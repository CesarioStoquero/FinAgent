using AwesomeAssertions;
using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Infrastructure.Projections;
using FinAgent.Wallet.IntegrationTests.Fixtures;
using MongoDB.Driver;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Projections;

/// <summary>
/// T18 — `[requer contêiner]`. Sem Docker, cada teste se auto-pula via
/// <c>Assert.SkipUnless</c>.
/// </summary>
public class LedgerProjectionTests(MongoFixture fx) : IClassFixture<MongoFixture>
{
    [Fact]
    public async Task Projetar_AccountOpened_CriaDocumentoComSaldoZeroEMoedaBrl()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var collection = CollectionFor(fx);
        var projection = new LedgerProjection(collection);
        var accountId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var evento = new AccountOpened(accountId, ownerId, Currencies.Brl, AccountType.Customer);

        await projection.ProjectAsync(evento, eventVersion: 0, ct);

        var document = await collection.Find(d => d.Id == accountId).SingleAsync(ct);
        document.BalanceCents.Should().Be(0);
        document.Currency.Should().Be(Currencies.Brl);
    }

    [Fact]
    public async Task Projetar_OMesmoAccountOpenedDuasVezes_NaoDuplicaNemAlteraODocumento()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var collection = CollectionFor(fx);
        var projection = new LedgerProjection(collection);
        var accountId = Guid.NewGuid();
        var evento = new AccountOpened(accountId, Guid.NewGuid(), Currencies.Brl, AccountType.Customer);

        await projection.ProjectAsync(evento, eventVersion: 0, ct);
        await projection.ProjectAsync(evento, eventVersion: 0, ct); // reentrega (Kafka at-least-once)

        var documentos = await collection.Find(d => d.Id == accountId).ToListAsync(ct);
        documentos.Should().ContainSingle();
        documentos[0].BalanceCents.Should().Be(0);
    }

    [Fact]
    public async Task Projetar_AccountOpened_NaoGravaNenhumDadoPessoal()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var collection = CollectionFor(fx);
        var projection = new LedgerProjection(collection);
        var accountId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var evento = new AccountOpened(accountId, ownerId, Currencies.Brl, AccountType.Customer);

        await projection.ProjectAsync(evento, eventVersion: 0, ct);

        var document = await collection.Find(d => d.Id == accountId).SingleAsync(ct);
        // Só ids opacos, moeda e saldo — nada de nome/CPF/e-mail/telefone (RF-10).
        document.Should().BeEquivalentTo(new
        {
            Id = accountId,
            OwnerId = ownerId,
            Currency = Currencies.Brl,
            BalanceCents = 0L,
            Version = 0,
        });
    }

    private static IMongoCollection<AccountDocument> CollectionFor(MongoFixture fx)
        => fx.Database.GetCollection<AccountDocument>($"accounts_{Guid.NewGuid():N}");
}
