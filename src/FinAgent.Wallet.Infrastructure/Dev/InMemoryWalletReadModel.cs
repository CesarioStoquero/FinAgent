using FinAgent.Wallet.Domain.Ports;

namespace FinAgent.Wallet.Infrastructure.Dev;

/// <summary>
/// Read model em memória (plano D5): lê o mesmo
/// <see cref="InMemoryWalletProjectionStore"/> alimentado pelo
/// <see cref="InMemoryEventPublisher"/>. Conveniência de execução local
/// (<c>Wallet:Store=InMemory</c>), NUNCA o default de produção.
/// </summary>
internal sealed class InMemoryWalletReadModel(InMemoryWalletProjectionStore projectionStore) : IWalletReadModel
{
    public Task<WalletView?> GetByIdAsync(Guid walletId, CancellationToken ct)
        => Task.FromResult(projectionStore.Get(walletId));
}
