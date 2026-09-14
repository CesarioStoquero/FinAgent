namespace FinAgent.Wallet.Domain;

/// <summary>
/// Envelope de um evento já gravado no event store: a versão atribuída e o
/// <c>occurred_at</c> do banco (plano D4). O evento de domínio em si permanece sem
/// timestamp, para que o replay continue determinístico.
/// </summary>
public sealed record EventEnvelope(Guid AggregateId, int Version, DateTimeOffset OccurredAt, IDomainEvent Event);
