namespace FinAgent.Wallet.Domain;

/// <summary>
/// Base de todo agregado event-sourced. Estado nunca é persistido direto — é
/// sempre derivado do replay dos eventos (ADR-0001). Nomes de método (<see cref="Raise"/>,
/// <see cref="Apply"/>, <see cref="Replay"/>, <see cref="Version"/>,
/// <see cref="UncommittedEvents"/>, <see cref="ClearUncommittedEvents"/>) são CONTRATO.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _uncommitted = [];

    public Guid Id { get; protected set; }

    public int Version { get; private set; } = -1;

    public IReadOnlyList<IDomainEvent> UncommittedEvents => _uncommitted;

    public void ClearUncommittedEvents() => _uncommitted.Clear();

    protected void Raise(IDomainEvent e)
    {
        Apply(e);
        _uncommitted.Add(e);
    }

    protected void Replay(IReadOnlyList<IDomainEvent> history)
    {
        foreach (var e in history)
        {
            Apply(e);
            Version++;
        }
    }

    protected abstract void Apply(IDomainEvent e);
}
