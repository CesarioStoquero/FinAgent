using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinAgent.Wallet.Infrastructure.Persistence;

/// <summary>
/// Permite ao `dotnet ef` (migrations add/database update) construir o
/// <see cref="WalletEventStoreDbContext"/> sem subir a Api (ADR-0011 seção 4).
/// A connection string vem da variável de ambiente <c>FINAGENT_POSTGRES_CONNECTION</c>,
/// com um default de dev que aponta para o Postgres do <c>docker-compose.yml</c> da raiz.
/// </summary>
internal sealed class WalletEventStoreDbContextFactory : IDesignTimeDbContextFactory<WalletEventStoreDbContext>
{
    private const string DevConnectionString =
        "Host=localhost;Port=5432;Database=finagent_events;Username=finagent;Password=finagent";

    public WalletEventStoreDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("FINAGENT_POSTGRES_CONNECTION")
            ?? DevConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder<WalletEventStoreDbContext>()
            .UseNpgsql(connectionString);

        return new WalletEventStoreDbContext(optionsBuilder.Options);
    }
}
