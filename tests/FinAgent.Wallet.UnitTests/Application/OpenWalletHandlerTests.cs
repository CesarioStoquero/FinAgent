using AwesomeAssertions;
using FinAgent.Wallet.Application.OpenWallet;
using FinAgent.Wallet.Domain;
using FinAgent.Wallet.UnitTests.Fakes;
using Xunit;

namespace FinAgent.Wallet.UnitTests.Application;

public class OpenWalletHandlerTests
{
    [Fact]
    public async Task Handle_TitularSemCarteira_GravaAccountOpenedNaVersaoZero()
    {
        var store = new InMemoryEventStore();
        var publisher = new RecordingEventPublisher();
        var handler = new OpenWalletHandler(store, publisher);
        var ownerId = Guid.NewGuid();

        var result = await handler.HandleAsync(new OpenWalletCommand(ownerId), TestContext.Current.CancellationToken);

        var history = await store.LoadAsync(result.WalletId, TestContext.Current.CancellationToken);
        history.Should().ContainSingle().Which.Should().BeOfType<AccountOpened>();
    }

    [Fact]
    public async Task Handle_TitularSemCarteira_PublicaOEventoDepoisDoAppend()
    {
        var store = new InMemoryEventStore();
        var publisher = new RecordingEventPublisher();
        var handler = new OpenWalletHandler(store, publisher);
        var ownerId = Guid.NewGuid();

        var result = await handler.HandleAsync(new OpenWalletCommand(ownerId), TestContext.Current.CancellationToken);

        publisher.Published.Should().ContainSingle();
        publisher.Published[0].AggregateId.Should().Be(result.WalletId);
        publisher.Published[0].Version.Should().Be(0);
    }

    [Fact]
    public async Task Handle_TitularSemCarteira_DevolveIdDerivadoDoTitularEMoedaBrl()
    {
        var store = new InMemoryEventStore();
        var publisher = new RecordingEventPublisher();
        var handler = new OpenWalletHandler(store, publisher);
        var ownerId = Guid.NewGuid();

        var result = await handler.HandleAsync(new OpenWalletCommand(ownerId), TestContext.Current.CancellationToken);

        result.WalletId.Should().Be(WalletIdentity.ForOwner(ownerId));
        result.OwnerId.Should().Be(ownerId);
        result.Currency.Should().Be(Currencies.Brl);
        result.AlreadyExisted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DepoisDoPublish_LimpaUncommittedEvents()
    {
        var store = new InMemoryEventStore();
        var publisher = new RecordingEventPublisher();
        var handler = new OpenWalletHandler(store, publisher);
        var ownerId = Guid.NewGuid();

        await handler.HandleAsync(new OpenWalletCommand(ownerId), TestContext.Current.CancellationToken);

        // LastAppendedEvents é a MESMA lista interna do agregado (UncommittedEvents
        // devolve a referência, não uma cópia): se o handler chamou
        // ClearUncommittedEvents depois do publish, ela aparece vazia aqui.
        store.LastAppendedEvents.Should().NotBeNull();
        store.LastAppendedEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_TitularQueJaTemCarteira_DevolveOMesmoIdEAlreadyExistedTrue()
    {
        var store = new InMemoryEventStore();
        var publisher = new RecordingEventPublisher();
        var handler = new OpenWalletHandler(store, publisher);
        var ownerId = Guid.NewGuid();
        var first = await handler.HandleAsync(new OpenWalletCommand(ownerId), TestContext.Current.CancellationToken);

        var second = await handler.HandleAsync(new OpenWalletCommand(ownerId), TestContext.Current.CancellationToken);

        second.WalletId.Should().Be(first.WalletId);
        second.AlreadyExisted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TitularQueJaTemCarteira_NaoFazAppendNemPublish()
    {
        var store = new InMemoryEventStore();
        var publisher = new RecordingEventPublisher();
        var handler = new OpenWalletHandler(store, publisher);
        var ownerId = Guid.NewGuid();
        await handler.HandleAsync(new OpenWalletCommand(ownerId), TestContext.Current.CancellationToken);
        var appendCallsAfterFirst = store.AppendCallCount;
        var publishedAfterFirst = publisher.Published.Count;

        await handler.HandleAsync(new OpenWalletCommand(ownerId), TestContext.Current.CancellationToken);

        store.AppendCallCount.Should().Be(appendCallsAfterFirst);
        publisher.Published.Should().HaveCount(publishedAfterFirst);
    }

    [Fact]
    public async Task Handle_TitularQueJaTemCarteira_NaoAlteraOSaldoDaCarteiraExistente()
    {
        var store = new InMemoryEventStore();
        var publisher = new RecordingEventPublisher();
        var handler = new OpenWalletHandler(store, publisher);
        var ownerId = Guid.NewGuid();
        var walletId = WalletIdentity.ForOwner(ownerId);
        var opened = new AccountOpened(walletId, ownerId, Currencies.Brl, AccountType.Customer);
        await store.AppendAsync(walletId, [opened], expectedVersion: -1, TestContext.Current.CancellationToken);
        await store.AppendAsync(walletId, [Deposit(walletId, 10_000)], expectedVersion: 0, TestContext.Current.CancellationToken);

        var result = await handler.HandleAsync(new OpenWalletCommand(ownerId), TestContext.Current.CancellationToken);

        result.AlreadyExisted.Should().BeTrue();
        var history = await store.LoadAsync(walletId, TestContext.Current.CancellationToken);
        var rehydrated = Account.Rehydrate(history);
        rehydrated.Balance.Cents.Should().Be(10_000);
    }

    [Fact]
    public async Task Handle_ComAberturaConcorrenteDoMesmoTitular_DevolveACarteiraExistente()
    {
        var store = new InMemoryEventStore();
        var publisher = new RecordingEventPublisher();
        var ownerId = Guid.NewGuid();
        var walletId = WalletIdentity.ForOwner(ownerId);

        // Simula uma abertura concorrente do MESMO titular: no meio do append do
        // handler sob teste, outra requisição "vence a corrida" e grava primeiro.
        store.BeforeAppend = async _ =>
        {
            store.BeforeAppend = null; // evita recursão infinita
            var concurrent = new OpenWalletHandler(store, new RecordingEventPublisher());
            await concurrent.HandleAsync(new OpenWalletCommand(ownerId), TestContext.Current.CancellationToken);
        };
        var handler = new OpenWalletHandler(store, publisher);

        var result = await handler.HandleAsync(new OpenWalletCommand(ownerId), TestContext.Current.CancellationToken);

        result.WalletId.Should().Be(walletId);
        result.AlreadyExisted.Should().BeTrue();
    }

    // Um depósito = credita a conta + debita a contrapartida do sistema (soma zero, RN-1).
    private static TransactionPosted Deposit(Guid accountId, long cents) =>
        new(Guid.NewGuid(),
        [
            new LedgerEntry(accountId, EntryDirection.Credit, cents),
            new LedgerEntry(SystemAccounts.ExternalFunding, EntryDirection.Debit, cents),
        ], "deposit");
}
