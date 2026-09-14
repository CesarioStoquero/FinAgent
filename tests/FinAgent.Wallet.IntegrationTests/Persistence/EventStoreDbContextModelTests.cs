using AwesomeAssertions;
using FinAgent.Wallet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Persistence;

/// <summary>
/// ADR-0011 — SEM contêiner: prova (1) que existe pelo menos uma migração no assembly
/// e (2) que o modelo do <see cref="WalletEventStoreDbContext"/> está em dia com as
/// migrações (nenhuma mudança de modelo ficou sem migração gerada — o risco que a
/// própria ADR lista). <c>GetMigrations()</c>/<c>HasPendingModelChanges()</c> comparam
/// snapshots estáticos; não abrem conexão com um banco de verdade.
/// </summary>
public class EventStoreDbContextModelTests
{
    private static WalletEventStoreDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WalletEventStoreDbContext>()
            .UseNpgsql("Host=localhost;Database=finagent_events;Username=finagent;Password=finagent")
            .Options;
        return new WalletEventStoreDbContext(options);
    }

    [Fact]
    public void Assembly_TemPeloMenosUmaMigracao()
    {
        using var dbContext = CreateDbContext();

        var migrations = dbContext.Database.GetMigrations();

        migrations.Should().NotBeEmpty();
    }

    [Fact]
    public void Modelo_NaoTemMudancaPendenteSemMigracaoGerada()
    {
        using var dbContext = CreateDbContext();

        var hasPendingChanges = dbContext.Database.HasPendingModelChanges();

        hasPendingChanges.Should().BeFalse();
    }
}
