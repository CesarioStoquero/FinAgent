using AwesomeAssertions;
using FinAgent.Wallet.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Persistence;

/// <summary>
/// ADR-0011 — `[requer contêiner]`. Sem Docker nesta máquina, todo teste se auto-pula
/// via <c>Assert.SkipUnless</c> (ver <see cref="DockerAvailability"/>) em vez de falhar.
/// O schema nasce por migração EF Core, não mais por DDL aplicada na subida — o fixture
/// já aplica a migração inicial em <see cref="PostgresFixture.InitializeAsync"/>.
/// </summary>
public class EventStoreMigrationsTests(PostgresFixture fx) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task Aplicar_DuasVezes_EhNoOp()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");

        // O fixture já migrou uma vez em InitializeAsync; o histórico do EF
        // (__EFMigrationsHistory) garante que reaplicar não roda a migração de novo.
        await using var dbContext = fx.CreateDbContext();

        var act = async () => await dbContext.Database.MigrateAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Tabela_TemChavePrimariaAggregateIdVersion()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        await using var connection = await fx.DataSource.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT a.attname
            FROM pg_index i
            JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY(i.indkey)
            WHERE i.indrelid = 'wallet_events'::regclass AND i.indisprimary
            ORDER BY array_position(i.indkey, a.attnum);
            """;

        var pkColumns = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            pkColumns.Add(reader.GetString(0));
        }

        pkColumns.Should().Equal("aggregate_id", "version");
    }

    [Fact]
    public async Task Update_EmEventoJaGravado_EhRecusadoPelaPermissao()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        // Grava um evento com a conexão de superusuário (setup) e tenta alterá-lo
        // com a role restrita da aplicação (RN-3: o passado é fisicamente imutável).
        var aggregateId = Guid.NewGuid();
        await using (var setup = await fx.DataSource.OpenConnectionAsync(ct))
        await using (var insert = setup.CreateCommand())
        {
            insert.CommandText =
                "INSERT INTO wallet_events (aggregate_id, version, event_type, payload) " +
                "VALUES (@id, 0, 'wallet.account-opened', '{}'::jsonb)";
            insert.Parameters.AddWithValue("id", aggregateId);
            await insert.ExecuteNonQueryAsync(ct);
        }

        await using var appConnection = new NpgsqlConnection(fx.ApplicationRoleConnectionString());
        await appConnection.OpenAsync(ct);
        await using var update = appConnection.CreateCommand();
        update.CommandText = "UPDATE wallet_events SET payload = '{\"tampered\":true}'::jsonb WHERE aggregate_id = @id";
        update.Parameters.AddWithValue("id", aggregateId);

        var act = async () => await update.ExecuteNonQueryAsync(ct);

        await act.Should().ThrowAsync<PostgresException>()
            .Where(e => e.SqlState == PostgresErrorCodes.InsufficientPrivilege);
    }
}
