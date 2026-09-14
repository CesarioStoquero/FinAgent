using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinAgent.Wallet.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração por Fluent API (ADR-0011: nunca Data Annotations) de
/// <see cref="WalletEventRow"/> — o schema físico de <c>wallet_events</c>
/// (ADR-0001, ADR-0007).
/// </summary>
internal sealed class WalletEventRowConfiguration : IEntityTypeConfiguration<WalletEventRow>
{
    public void Configure(EntityTypeBuilder<WalletEventRow> builder)
    {
        builder.ToTable("wallet_events");

        // PK (aggregate_id, version) é a concorrência otimista (ADR-0007): duas escritas
        // disputando a mesma versão do mesmo stream fazem a PK recusar a perdedora.
        builder.HasKey(e => new { e.AggregateId, e.Version });

        // BIGSERIAL (não identity column): reproduz exatamente o `001_event_store.sql`
        // original, cuja sequência (nomeada por convenção do Postgres,
        // wallet_events_global_seq_seq) é referenciada pelo GRANT de sequência.
        builder.Property(e => e.GlobalSeq)
            .HasColumnName("global_seq")
            .UseSerialColumn()
            .ValueGeneratedOnAdd();

        builder.Property(e => e.AggregateId)
            .HasColumnName("aggregate_id")
            .HasColumnType("uuid");

        builder.Property(e => e.Version)
            .HasColumnName("version");

        // Nome de CONTRATO (ex.: wallet.account-opened), nunca o nome CLR.
        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        // global_seq é a ordem física de escrita, base de um catch-up/reprojeção futuro.
        builder.HasIndex(e => e.GlobalSeq)
            .IsUnique()
            .HasDatabaseName("ux_wallet_events_global_seq");

        builder.HasIndex(e => e.AggregateId)
            .HasDatabaseName("ix_wallet_events_aggregate_id");
    }
}
