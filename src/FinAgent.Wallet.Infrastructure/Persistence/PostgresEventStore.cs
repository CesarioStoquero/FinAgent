using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Domain.Ports;
using FinAgent.Wallet.Infrastructure.Serialization;
using Npgsql;

namespace FinAgent.Wallet.Infrastructure.Persistence;

/// <summary>
/// Único adapter que enxerga <c>Npgsql</c> (ADR-0003). Append-only com concorrência
/// otimista (ADR-0007): a PK <c>(aggregate_id, version)</c> recusa a escrita perdedora,
/// que vira <see cref="ConcurrencyException"/>. Todos os eventos de um append são
/// UMA transação — RF-11 (nada parcialmente criado) sai da atomicidade do commit.
/// </summary>
internal sealed class PostgresEventStore(NpgsqlDataSource dataSource) : IEventStore
{
    public async Task<IReadOnlyList<IDomainEvent>> LoadAsync(Guid aggregateId, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT event_type, payload FROM wallet_events WHERE aggregate_id = @aggregateId ORDER BY version";
        command.Parameters.AddWithValue("aggregateId", aggregateId);

        var events = new List<IDomainEvent>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var eventType = reader.GetString(0);
            var payload = reader.GetString(1);
            events.Add(JsonEventSerializer.Deserialize(eventType, payload));
        }

        return events;
    }

    public async Task<IReadOnlyList<EventEnvelope>> AppendAsync(
        Guid aggregateId, IReadOnlyList<IDomainEvent> events, int expectedVersion, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        var appended = new List<EventEnvelope>(events.Count);
        var version = expectedVersion;

        try
        {
            foreach (var domainEvent in events)
            {
                version++;
                var (eventType, payload) = JsonEventSerializer.Serialize(domainEvent);

                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = """
                    INSERT INTO wallet_events (aggregate_id, version, event_type, payload)
                    VALUES (@aggregateId, @version, @eventType, @payload::jsonb)
                    RETURNING occurred_at;
                    """;
                command.Parameters.AddWithValue("aggregateId", aggregateId);
                command.Parameters.AddWithValue("version", version);
                command.Parameters.AddWithValue("eventType", eventType);
                command.Parameters.AddWithValue("payload", payload);

                var occurredAt = (DateTimeOffset)(await command.ExecuteScalarAsync(ct))!;
                appended.Add(new EventEnvelope(aggregateId, version, occurredAt, domainEvent));
            }

            await transaction.CommitAsync(ct);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            await transaction.RollbackAsync(ct);
            throw new ConcurrencyException(aggregateId, expectedVersion);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return appended;
    }
}
