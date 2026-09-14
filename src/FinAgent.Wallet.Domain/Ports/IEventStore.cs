namespace FinAgent.Wallet.Domain.Ports;

/// <summary>
/// Porta do event store (ADR-0007). O núcleo declara O QUE precisa; o adapter
/// Postgres mora na Infrastructure.
/// </summary>
public interface IEventStore
{
    public Task<IReadOnlyList<IDomainEvent>> LoadAsync(Guid aggregateId, CancellationToken ct);

    // Lança ConcurrencyException se a versão no banco != expectedVersion (plano D4:
    // devolve os envelopes gravados, com versão atribuída e occurred_at do banco).
    public Task<IReadOnlyList<EventEnvelope>> AppendAsync(
        Guid aggregateId, IReadOnlyList<IDomainEvent> events, int expectedVersion, CancellationToken ct);
}
