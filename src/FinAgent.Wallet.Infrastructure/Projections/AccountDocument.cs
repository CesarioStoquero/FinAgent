namespace FinAgent.Wallet.Infrastructure.Projections;

/// <summary>
/// Documento do read model no Mongo (um por conta, ADR-0005). Descartável: a
/// verdade é o event store — este documento pode ser reprojetado do zero.
/// </summary>
internal sealed class AccountDocument
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Currency { get; set; } = "";

    public long BalanceCents { get; set; }

    // Versão do último evento aplicado (idempotência — Kafka é at-least-once).
    public int Version { get; set; }
}
