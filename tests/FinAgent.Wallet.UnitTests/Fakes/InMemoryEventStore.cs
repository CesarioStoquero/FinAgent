using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Domain.Ports;

namespace FinAgent.Wallet.UnitTests.Fakes;

/// <summary>
/// Test double fiel ao <see cref="IEventStore"/>: guarda streams por
/// <c>aggregateId</c>, atribui versões sequenciais e lança
/// <see cref="ConcurrencyException"/> quando <c>expectedVersion</c> não bate —
/// exatamente as regras que o handler depende do Postgres real.
/// </summary>
public sealed class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<Guid, List<EventEnvelope>> _streams = [];

    /// <summary>
    /// Hook para simular uma escrita concorrente: executado depois de conferir a
    /// versão esperada e antes de gravar. Permite ao teste injetar um append
    /// concorrente no meio do caminho.
    /// </summary>
    public Func<Guid, Task>? BeforeAppend { get; set; }

    /// <summary>
    /// Referência da última lista de eventos recebida em <see cref="AppendAsync"/>,
    /// para o teste observar de fora se o chamador limpou (<c>ClearUncommittedEvents</c>)
    /// o agregado depois do append — a mesma lista é a instância interna do agregado.
    /// </summary>
    public IReadOnlyList<IDomainEvent>? LastAppendedEvents { get; private set; }

    /// <summary>Quantas vezes <see cref="AppendAsync"/> foi CHAMADO (não quantas gravou).</summary>
    public int AppendCallCount { get; private set; }

    public Task<IReadOnlyList<IDomainEvent>> LoadAsync(Guid aggregateId, CancellationToken ct)
    {
        var history = _streams.TryGetValue(aggregateId, out var envelopes)
            ? envelopes.Select(e => e.Event).ToList()
            : [];
        return Task.FromResult<IReadOnlyList<IDomainEvent>>(history);
    }

    public async Task<IReadOnlyList<EventEnvelope>> AppendAsync(
        Guid aggregateId, IReadOnlyList<IDomainEvent> events, int expectedVersion, CancellationToken ct)
    {
        AppendCallCount++;
        LastAppendedEvents = events;
        var stream = _streams.TryGetValue(aggregateId, out var existing) ? existing : [];
        var currentVersion = stream.Count - 1;

        if (BeforeAppend is { } hook)
        {
            await hook(aggregateId);
        }

        // Releitura pós-hook: o append concorrente injetado pelo hook pode ter
        // mudado a versão do stream.
        stream = _streams.TryGetValue(aggregateId, out var afterHook) ? afterHook : stream;
        currentVersion = stream.Count - 1;

        if (currentVersion != expectedVersion)
        {
            throw new ConcurrencyException(aggregateId, expectedVersion);
        }

        var appended = new List<EventEnvelope>();
        var version = currentVersion;
        foreach (var e in events)
        {
            version++;
            appended.Add(new EventEnvelope(aggregateId, version, DateTimeOffset.UtcNow, e));
        }

        stream.AddRange(appended);
        _streams[aggregateId] = stream;

        return appended;
    }
}
