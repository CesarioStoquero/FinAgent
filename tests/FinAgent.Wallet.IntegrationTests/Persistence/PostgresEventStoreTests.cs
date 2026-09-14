using AwesomeAssertions;
using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Infrastructure.Persistence;
using FinAgent.Wallet.IntegrationTests.Fixtures;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Persistence;

/// <summary>
/// T16 — `[requer contêiner]`. Base: `reference/integration.md`. Sem Docker, cada
/// teste se auto-pula via <c>Assert.SkipUnless</c>.
/// </summary>
public class PostgresEventStoreTests(PostgresFixture fx) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task Append_DepoisLoad_RetornaEventosNaOrdem()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var store = new PostgresEventStore(fx.DataSource);
        var ownerId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var opened = new AccountOpened(id, ownerId, Currencies.Brl, AccountType.Customer);

        await store.AppendAsync(id, [opened], expectedVersion: -1, ct);
        var history = await store.LoadAsync(id, ct);

        history.Should().ContainSingle().Which.Should().Be(opened);
    }

    [Fact]
    public async Task Append_DuasEscritasNaMesmaVersao_UmaFalhaPorConcorrencia()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var store = new PostgresEventStore(fx.DataSource);
        var ownerId = Guid.NewGuid();
        var id = Guid.NewGuid();
        await store.AppendAsync(id, [new AccountOpened(id, ownerId, Currencies.Brl, AccountType.Customer)], -1, ct);

        var a = store.AppendAsync(id, [Dep(id, 100)], 0, ct);
        var b = store.AppendAsync(id, [Dep(id, 200)], 0, ct);

        var act = async () => await Task.WhenAll(a, b);
        await act.Should().ThrowAsync<ConcurrencyException>();
    }

    [Fact]
    public async Task Append_ComFalhaNoMeio_NaoDeixaEventoParcial()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var store = new PostgresEventStore(fx.DataSource);
        var ownerId = Guid.NewGuid();
        var id = Guid.NewGuid();

        // Grava a versão 0 antes, fora do append em teste.
        await store.AppendAsync(id, [new AccountOpened(id, ownerId, Currencies.Brl, AccountType.Customer)], -1, ct);

        // Este append assume expectedVersion 0 (correto) mas o SEGUNDO evento da lista
        // reaproveita uma versão já ocupada por fora — força a violação de PK no meio
        // do lote, dentro da mesma transação.
        var conflicting = Dep(id, 500);
        await store.AppendAsync(id, [conflicting], 0, ct); // ocupa a versão 1

        var act = async () => await store.AppendAsync(
            id, [Dep(id, 100), Dep(id, 200)], expectedVersion: 0, ct); // 1ª tentaria versão 1 (ocupada), 2ª versão 2

        await act.Should().ThrowAsync<ConcurrencyException>();

        var history = await store.LoadAsync(id, ct);
        history.Should().HaveCount(2); // AccountOpened + o depósito que já tinha vencido — nada do lote falho entrou
    }

    [Fact]
    public async Task Load_DeStreamInexistente_DevolveListaVazia()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var store = new PostgresEventStore(fx.DataSource);

        var history = await store.LoadAsync(Guid.NewGuid(), ct);

        history.Should().BeEmpty();
    }

    // Um depósito balanceado (partida dobrada, ADR-0008) — helper de reference/integration.md.
    private static TransactionPosted Dep(Guid accountId, long cents) =>
        new(Guid.NewGuid(), new[]
        {
            new LedgerEntry(accountId, EntryDirection.Credit, cents),
            new LedgerEntry(SystemAccounts.ExternalFunding, EntryDirection.Debit, cents),
        }, "deposit");
}
