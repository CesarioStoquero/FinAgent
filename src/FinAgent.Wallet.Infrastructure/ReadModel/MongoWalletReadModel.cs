using FinAgent.Wallet.Domain.Ports;
using FinAgent.Wallet.Infrastructure.Projections;
using MongoDB.Driver;

namespace FinAgent.Wallet.Infrastructure.ReadModel;

/// <summary>
/// Único adapter (além da projeção) que enxerga <c>MongoDB.Driver</c> para leitura
/// (ADR-0003, ADR-0005). Id nunca aberto devolve <see langword="null"/> — é o que
/// permite ao chamador recusar com "carteira inexistente" (RN-12).
/// </summary>
internal sealed class MongoWalletReadModel(IMongoCollection<AccountDocument> collection) : IWalletReadModel
{
    public async Task<WalletView?> GetByIdAsync(Guid walletId, CancellationToken ct)
    {
        var document = await collection.Find(d => d.Id == walletId).FirstOrDefaultAsync(ct);

        return document is null
            ? null
            : new WalletView(document.Id, document.OwnerId, document.Currency, document.BalanceCents);
    }
}
