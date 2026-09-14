using AwesomeAssertions;
using FinAgent.Wallet.Domain;
using Xunit;

namespace FinAgent.Wallet.UnitTests.Fakes;

public class InMemoryEventStoreTests
{
    [Fact]
    public async Task Append_EmStreamNovo_ComVersaoEsperadaMenosUm_Grava()
    {
        var store = new InMemoryEventStore();
        var id = Guid.NewGuid();

        var envelopes = await store.AppendAsync(
            id, new IDomainEvent[] { new AccountOpened(id, Guid.NewGuid(), Currencies.Brl, AccountType.Customer) },
            expectedVersion: -1, TestContext.Current.CancellationToken);

        envelopes.Should().ContainSingle().Which.Version.Should().Be(0);
    }

    [Fact]
    public async Task Append_ComVersaoEsperadaErrada_LancaConcurrencyException()
    {
        var store = new InMemoryEventStore();
        var id = Guid.NewGuid();
        await store.AppendAsync(
            id, new IDomainEvent[] { new AccountOpened(id, Guid.NewGuid(), Currencies.Brl, AccountType.Customer) },
            expectedVersion: -1, TestContext.Current.CancellationToken);

        var act = () => store.AppendAsync(
            id, new IDomainEvent[] { new AccountOpened(id, Guid.NewGuid(), Currencies.Brl, AccountType.Customer) },
            expectedVersion: -1, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ConcurrencyException>();
    }

    [Fact]
    public async Task Load_DepoisDeAppend_DevolveEventosNaOrdem()
    {
        var store = new InMemoryEventStore();
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var opened = new AccountOpened(id, ownerId, Currencies.Brl, AccountType.Customer);
        await store.AppendAsync(id, new IDomainEvent[] { opened }, expectedVersion: -1, TestContext.Current.CancellationToken);

        var history = await store.LoadAsync(id, TestContext.Current.CancellationToken);

        history.Should().ContainSingle().Which.Should().Be(opened);
    }
}
