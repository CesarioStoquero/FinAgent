using FinAgent.Wallet.Domain;
using MongoDB.Driver;

namespace FinAgent.Wallet.Infrastructure.Projections;

/// <summary>
/// Projeta eventos do ledger no read model Mongo (template canônico
/// <c>reference/projection.md</c>). Nesta feature trata SÓ <see cref="AccountOpened"/> —
/// <see cref="TransactionPosted"/> chega com BL-1. Upsert idempotente: reprocessar a
/// mesma mensagem (Kafka é at-least-once) não altera o documento.
/// </summary>
internal sealed class LedgerProjection(IMongoCollection<AccountDocument> collection)
{
    public async Task ProjectAsync(IDomainEvent domainEvent, int eventVersion, CancellationToken ct)
    {
        switch (domainEvent)
        {
            case AccountOpened ev:
                await collection.ReplaceOneAsync(
                    d => d.Id == ev.AccountId,
                    new AccountDocument
                    {
                        Id = ev.AccountId,
                        OwnerId = ev.OwnerId,
                        Currency = ev.Currency,
                        BalanceCents = 0,
                        Version = eventVersion,
                    },
                    new ReplaceOptions { IsUpsert = true },
                    ct);
                break;

            // TransactionPosted: nenhum lançamento é postado por esta feature (a
            // abertura não move dinheiro) — o case fica fora de propósito até BL-1.
        }
    }
}
