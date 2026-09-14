namespace FinAgent.Wallet.Domain.Ports;

/// <summary>
/// Porta de publicação de eventos (Kafka, ADR-0004). Recebe envelopes (plano D4),
/// não eventos crus, para que a projeção tenha a versão sem inferir nada.
/// </summary>
public interface IEventPublisher
{
    public Task PublishAsync(IReadOnlyList<EventEnvelope> envelopes, CancellationToken ct);
}
