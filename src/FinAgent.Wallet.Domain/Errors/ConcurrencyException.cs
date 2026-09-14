namespace FinAgent.Wallet.Domain;

/// <summary>
/// Conflito de concorrência otimista no append do event store (ADR-0007):
/// a versão esperada não bateu com a versão atual do stream.
/// </summary>
public sealed class ConcurrencyException(Guid aggregateId, int expectedVersion)
    : DomainException($"Conflito de concorrência na carteira {aggregateId}: versão esperada {expectedVersion}.")
{
    public Guid AggregateId { get; } = aggregateId;

    public int ExpectedVersion { get; } = expectedVersion;
}
