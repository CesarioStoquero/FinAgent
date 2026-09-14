using System.Text.Json;
using Confluent.Kafka;
using FinAgent.Wallet.Infrastructure.Projections;
using FinAgent.Wallet.Infrastructure.Serialization;
using Microsoft.Extensions.Hosting;

namespace FinAgent.Wallet.Infrastructure.Messaging;

/// <summary>
/// Consome <see cref="KafkaEventPublisher.Topic"/>, desserializa pelo
/// <see cref="EventTypeRegistry"/> e projeta (T18). Commit SÓ depois de projetar
/// (<c>enable.auto.commit=false</c>, plano D7): at-least-once + projeção idempotente.
/// Hospedado no próprio processo da Wallet Api (plano D8).
/// </summary>
internal sealed class WalletProjectionWorker(string bootstrapServers, LedgerProjection projection) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "finagent.wallet.projection",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        }).Build();

        consumer.Subscribe(KafkaEventPublisher.Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result?.Message is null)
                {
                    continue;
                }

                var message = JsonSerializer.Deserialize<WalletEventMessage>(result.Message.Value)
                    ?? throw new InvalidOperationException("Mensagem do tópico veio vazia.");
                var domainEvent = JsonEventSerializer.Deserialize(message.EventType, message.Payload);

                await projection.ProjectAsync(domainEvent, message.Version, stoppingToken);

                consumer.Commit(result);
            }
        }
        catch (OperationCanceledException)
        {
            // Encerramento normal do host (stoppingToken cancelado).
        }
        finally
        {
            consumer.Close();
        }
    }
}
