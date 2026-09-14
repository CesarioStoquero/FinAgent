using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Domain.Ports;

namespace FinAgent.Wallet.Infrastructure.Dev;

/// <summary>
/// Event store em memória (plano D5): conveniência de execução local sem Docker
/// (<c>Wallet:Store=InMemory</c>), NUNCA o default de produção (esse é
/// <c>PostgresEventStore</c>). Mesmas regras de concorrência otimista do Postgres:
/// PK lógica <c>(aggregateId, version)</c> via lock.
/// </summary>
internal sealed class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<Guid, List<EventEnvelope>> _streams = [];
    private readonly Lock _lock = new();

    public Task<IReadOnlyList<IDomainEvent>> LoadAsync(Guid aggregateId, CancellationToken ct)
    {
        lock (_lock)
        {
            IReadOnlyList<IDomainEvent> history = _streams.TryGetValue(aggregateId, out var envelopes)
                ? envelopes.Select(e => e.Event).ToList()
                : [];
            return Task.FromResult(history);
        }
    }

    public Task<IReadOnlyList<EventEnvelope>> AppendAsync(
        Guid aggregateId, IReadOnlyList<IDomainEvent> events, int expectedVersion, CancellationToken ct)
    {
        lock (_lock)
        {
            var stream = _streams.TryGetValue(aggregateId, out var existing) ? existing : [];
            var currentVersion = stream.Count - 1;

            if (currentVersion != expectedVersion)
            {
                throw new ConcurrencyException(aggregateId, expectedVersion);
            }

            var appended = new List<EventEnvelope>(events.Count);
            var version = currentVersion;
            foreach (var domainEvent in events)
            {
                version++;
                appended.Add(new EventEnvelope(aggregateId, version, DateTimeOffset.UtcNow, domainEvent));
            }

            stream.AddRange(appended);
            _streams[aggregateId] = stream;

            return Task.FromResult<IReadOnlyList<EventEnvelope>>(appended);
        }
    }
}
