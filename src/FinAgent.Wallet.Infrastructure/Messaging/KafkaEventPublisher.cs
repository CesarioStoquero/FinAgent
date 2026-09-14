using System.Text.Json;
using Confluent.Kafka;
using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Domain.Ports;
using FinAgent.Wallet.Infrastructure.Serialization;

namespace FinAgent.Wallet.Infrastructure.Messaging;

/// <summary>
/// Único adapter que enxerga <c>Confluent.Kafka</c> (ADR-0003). Publica cada
/// <see cref="EventEnvelope"/> no tópico <see cref="Topic"/> com key = aggregateId
/// (plano D7), garantindo ordem por carteira dentro da partição.
/// </summary>
internal sealed class KafkaEventPublisher : IEventPublisher, IAsyncDisposable
{
    public const string Topic = "finagent.wallet.events";

    private readonly IProducer<string, string> _producer;

    public KafkaEventPublisher(string bootstrapServers)
        : this(new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = bootstrapServers }).Build())
    {
    }

    // Ponto de extensão para teste injetar um IProducer configurado à mão, se precisar.
    internal KafkaEventPublisher(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishAsync(IReadOnlyList<EventEnvelope> envelopes, CancellationToken ct)
    {
        foreach (var envelope in envelopes)
        {
            var (eventType, payload) = JsonEventSerializer.Serialize(envelope.Event);

            // Mensagem carrega só aggregateId, version, occurredAt, eventType, payload —
            // nenhum dado pessoal (RF-10/LGPD-9). O payload em si já respeita isso porque
            // vem do evento de domínio, que nunca carrega PII.
            var message = new WalletEventMessage(envelope.AggregateId, envelope.Version, envelope.OccurredAt, eventType, payload);
            var json = JsonSerializer.Serialize(message);

            await _producer.ProduceAsync(
                Topic,
                new Message<string, string> { Key = envelope.AggregateId.ToString(), Value = json },
                ct);
        }
    }

    public ValueTask DisposeAsync()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
        return ValueTask.CompletedTask;
    }
}

/// <summary>Formato da mensagem publicada no tópico (plano D7).</summary>
internal sealed record WalletEventMessage(Guid AggregateId, int Version, DateTimeOffset OccurredAt, string EventType, string Payload);
