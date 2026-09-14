using AwesomeAssertions;
using FinAgent.Wallet.Domain;
using Xunit;

namespace FinAgent.Wallet.UnitTests.Domain;

/// <summary>
/// Agregado-dublê minúsculo, só para exercitar a base <see cref="AggregateRoot"/>
/// sem depender de nenhum evento concreto do módulo.
/// </summary>
file sealed record DummyEvent : IDomainEvent;

file sealed class DummyAggregate : AggregateRoot
{
    public int AppliedCount { get; private set; }

    public static DummyAggregate New()
    {
        var a = new DummyAggregate();
        a.Raise(new DummyEvent());
        return a;
    }

    public static DummyAggregate Rehydrate(IReadOnlyList<IDomainEvent> history)
    {
        var a = new DummyAggregate();
        a.Replay(history);
        return a;
    }

    protected override void Apply(IDomainEvent e) => AppliedCount++;
}

public class AggregateRootTests
{
    [Fact]
    public void Version_SemHistorico_EhMenosUm()
    {
        var aggregate = DummyAggregate.Rehydrate([]);

        aggregate.Version.Should().Be(-1);
    }

    [Fact]
    public void Replay_ComDoisEventos_DeixaVersaoUm()
    {
        var aggregate = DummyAggregate.Rehydrate([new DummyEvent(), new DummyEvent()]);

        aggregate.Version.Should().Be(1);
    }

    [Fact]
    public void ClearUncommittedEvents_DepoisDeRaise_EsvaziaSemMudarEstado()
    {
        var aggregate = DummyAggregate.New();
        aggregate.UncommittedEvents.Should().ContainSingle();

        aggregate.ClearUncommittedEvents();

        aggregate.UncommittedEvents.Should().BeEmpty();
        aggregate.AppliedCount.Should().Be(1);
    }
}
