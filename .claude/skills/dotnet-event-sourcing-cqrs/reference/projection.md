# Template canônico — Projeção (Kafka → read model Mongo, partida dobrada)

A projeção é um adapter de Infrastructure. Consome `TransactionPosted` do Kafka e aplica
CADA lançamento ao saldo da conta correspondente no Mongo — inclusive a conta de
contrapartida do sistema (é aqui que o saldo dela existe, ADR-0008). Upsert idempotente.

```csharp
namespace FinAgent.Wallet.Infrastructure.Projections;

// Documento do read model no Mongo (um por conta).
internal sealed class AccountDocument
{
    public Guid Id { get; set; }
    public string Currency { get; set; } = "";
    public long BalanceCents { get; set; }
    public int Version { get; set; } // versão do último evento aplicado (idempotência)
}

internal sealed class LedgerProjection(IMongoCollection<AccountDocument> col)
{
    // Uma vez por evento consumido. DEVE ser idempotente (Kafka entrega ao menos uma vez).
    public async Task ProjectAsync(IDomainEvent e, int eventVersion, CancellationToken ct)
    {
        switch (e)
        {
            case AccountOpened ev:
                await col.ReplaceOneAsync(
                    d => d.Id == ev.AccountId,
                    new AccountDocument { Id = ev.AccountId, Currency = ev.Currency, BalanceCents = 0, Version = eventVersion },
                    new ReplaceOptions { IsUpsert = true }, ct);
                break;

            case TransactionPosted ev:
                // Aplica os DOIS lados: crédito soma, débito subtrai, em cada conta tocada.
                foreach (var entry in ev.Entries)
                {
                    long delta = entry.Direction == EntryDirection.Credit
                        ? +entry.AmountCents
                        : -entry.AmountCents;
                    await ApplyDeltaAsync(entry.AccountId, delta, eventVersion, ct);
                }
                break;
        }
    }

    // Só aplica se o evento é mais novo que o já projetado para aquela conta (idempotência).
    private Task ApplyDeltaAsync(Guid accountId, long deltaCents, int eventVersion, CancellationToken ct)
        => col.UpdateOneAsync(
            d => d.Id == accountId && d.Version < eventVersion,
            Builders<AccountDocument>.Update
                .Inc(d => d.BalanceCents, deltaCents)
                .Set(d => d.Version, eventVersion),
            new UpdateOptions { IsUpsert = true }, // upsert: cria a conta de contrapartida na 1ª vez
            ct);
}
```

## Verificação de consistência (opcional, mas ótimo num ledger)

Como RN-1 garante soma zero em cada transação, a soma de TODOS os saldos de conta deve ser
sempre 0. Um teste/health-check pode somar `BalanceCents` de todas as contas e afirmar `== 0`.

## Erros que a IA comete aqui (evite)

- Projetar só a conta do cliente e ignorar a de contrapartida — o saldo do sistema mora
  aqui; sem os dois lados a soma global deixa de ser zero.
- Projeção não idempotente: sem o guard `Version < eventVersion`, reprocessar dobra o saldo.
- Projeção com regra de negócio (recusar saldo negativo etc.) — isso já foi decidido no
  write side; a projeção só reflete.
