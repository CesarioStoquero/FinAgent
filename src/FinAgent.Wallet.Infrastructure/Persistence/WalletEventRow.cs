namespace FinAgent.Wallet.Infrastructure.Persistence;

/// <summary>
/// Mapeamento EF da linha física de <c>wallet_events</c> (ADR-0011). Existe só para o
/// EF Core DEFINIR o schema via migração — <see cref="Persistence.PostgresEventStore"/>
/// continua lendo/escrevendo com Npgsql cru, nunca por este tipo (ADR-0011 seção 3).
/// </summary>
internal sealed class WalletEventRow
{
    public long GlobalSeq { get; set; }

    public Guid AggregateId { get; set; }

    public int Version { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }
}
