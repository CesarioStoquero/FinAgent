using FinAgent.Wallet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Fixtures;

/// <summary>
/// Sobe um Postgres real via Testcontainers e aplica as migrações EF Core do event
/// store (ADR-0011). Template canônico: <c>dotnet-testing/reference/integration.md</c>.
/// Sem Docker, <see cref="InitializeAsync"/> não constrói nem inicia o container (evita
/// a <c>DockerUnavailableException</c> que <c>Build()</c> lançaria de cara) — os testes
/// que dependem deste fixture chamam
/// <c>Assert.SkipUnless(DockerAvailability.IsAvailable, ...)</c> antes de tocar
/// <see cref="DataSource"/>.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public NpgsqlDataSource DataSource { get; private set; } = null!;

    public string ConnectionString => _container!.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        if (!DockerAvailability.IsAvailable)
        {
            return;
        }

        _container = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await _container.StartAsync();
        DataSource = NpgsqlDataSource.Create(ConnectionString);

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (DataSource is not null)
        {
            await DataSource.DisposeAsync();
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Novo <see cref="WalletEventStoreDbContext"/> apontando para o Postgres do
    /// fixture — usado pelos testes que precisam reaplicar a migração (para provar que
    /// o histórico do EF a torna no-op).
    /// </summary>
    internal WalletEventStoreDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WalletEventStoreDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new WalletEventStoreDbContext(options);
    }

    /// <summary>
    /// Connection string autenticada como a role restrita da aplicação (RN-3) —
    /// só SELECT/INSERT, usada para provar que UPDATE/DELETE são recusados.
    /// </summary>
    public string ApplicationRoleConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder(ConnectionString)
        {
            Username = WalletApplicationRole.Name,
            Password = WalletApplicationRole.Password,
        };
        return builder.ConnectionString;
    }
}
