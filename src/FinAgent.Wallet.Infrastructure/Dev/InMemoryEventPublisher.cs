using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Domain.Ports;

namespace FinAgent.Wallet.Infrastructure.Dev;

/// <summary>
/// Publicador em memória (plano D5): sem Kafka, projeta de forma síncrona no
/// <see cref="InMemoryWalletProjectionStore"/> compartilhado com
/// <see cref="InMemoryWalletReadModel"/>. Conveniência de execução local
/// (<c>Wallet:Store=InMemory</c>), NUNCA o default de produção.
/// </summary>
internal sealed class InMemoryEventPublisher(InMemoryWalletProjectionStore projectionStore) : IEventPublisher
{
    public Task PublishAsync(IReadOnlyList<EventEnvelope> envelopes, CancellationToken ct)
    {
        foreach (var envelope in envelopes)
        {
            projectionStore.Apply(envelope);
        }

        return Task.CompletedTask;
    }
}
