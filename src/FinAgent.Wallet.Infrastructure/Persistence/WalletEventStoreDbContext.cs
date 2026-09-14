using FinAgent.Wallet.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FinAgent.Wallet.Infrastructure.Persistence;

/// <summary>
/// DbContext do event store (ADR-0011). Existe SÓ para definir e migrar o schema de
/// <c>wallet_events</c> — não é caminho de dados. <see cref="PostgresEventStore"/>
/// continua fazendo <c>LoadAsync</c>/<c>AppendAsync</c> com Npgsql cru (ADR-0011 seção 3).
/// </summary>
internal sealed class WalletEventStoreDbContext(DbContextOptions<WalletEventStoreDbContext> options)
    : DbContext(options)
{
    public DbSet<WalletEventRow> WalletEvents => Set<WalletEventRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new WalletEventRowConfiguration());
    }
}
