using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Domain.Ports;

namespace FinAgent.Wallet.UnitTests.Fakes;

/// <summary>
/// Test double de <see cref="IEventPublisher"/> que só grava o que recebeu, para
/// o teste verificar a ordem append -&gt; publish e o conteúdo publicado.
/// </summary>
public sealed class RecordingEventPublisher : IEventPublisher
{
    private readonly List<EventEnvelope> _published = [];

    public IReadOnlyList<EventEnvelope> Published => _published;

    public Task PublishAsync(IReadOnlyList<EventEnvelope> envelopes, CancellationToken ct)
    {
        _published.AddRange(envelopes);
        return Task.CompletedTask;
    }
}
