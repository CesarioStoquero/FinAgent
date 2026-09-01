# Template canônico — Query + Handler (read side)

Query NUNCA toca o event store nem reidrata agregado. Lê o read model (Mongo, ADR-0005).

## Contratos

```csharp
namespace FinAgent.Wallet.Application;

public interface IQuery<TResult>;
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct);
}
```

## Porta do read model (Domain) + DTO de leitura (Application)

```csharp
namespace FinAgent.Wallet.Domain.Ports;

public interface IWalletReadModel
{
    Task<WalletView?> GetByIdAsync(Guid walletId, CancellationToken ct);
}

// DTO de leitura — plano, feito para a UI/agente. NÃO é o agregado.
public sealed record WalletView(Guid WalletId, string Currency, long BalanceCents);
```

## Handler (só lê)

```csharp
namespace FinAgent.Wallet.Application.GetBalance;

public sealed record GetBalanceQuery(Guid WalletId) : IQuery<WalletView?>;

public sealed class GetBalanceHandler(IWalletReadModel readModel)
    : IQueryHandler<GetBalanceQuery, WalletView?>
{
    public Task<WalletView?> HandleAsync(GetBalanceQuery q, CancellationToken ct)
        => readModel.GetByIdAsync(q.WalletId, ct);
}
```

## Erros que a IA comete aqui (evite)

- Chamar `IEventStore.LoadAsync` numa query para "calcular o saldo na hora" — proibido.
  Se o read model está atrasado, o problema é a projeção, não a query.
- Retornar o agregado de domínio — devolva sempre um DTO de leitura (`WalletView`).
