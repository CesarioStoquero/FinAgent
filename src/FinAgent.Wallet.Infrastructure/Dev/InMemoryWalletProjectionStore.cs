using System.Collections.Concurrent;
using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Domain.Ports;

namespace FinAgent.Wallet.Infrastructure.Dev;

/// <summary>
/// Estado compartilhado entre <see cref="InMemoryEventPublisher"/> e
/// <see cref="InMemoryWalletReadModel"/> (plano D5): conveniência de execução local
/// sem Docker (<c>Wallet:Store=InMemory</c>), NUNCA o default de produção. Substitui
/// o Kafka + Mongo reais projetando de forma síncrona e em processo.
/// </summary>
internal sealed class InMemoryWalletProjectionStore
{
    private readonly ConcurrentDictionary<Guid, WalletView> _views = new();

    public void Apply(EventEnvelope envelope)
    {
        if (envelope.Event is AccountOpened ev)
        {
            _views[ev.AccountId] = new WalletView(ev.AccountId, ev.OwnerId, ev.Currency, BalanceCents: 0);
        }

        // TransactionPosted: fora de propósito nesta feature (a abertura não move
        // dinheiro) — chega com BL-1.
    }

    public WalletView? Get(Guid walletId) => _views.GetValueOrDefault(walletId);
}
