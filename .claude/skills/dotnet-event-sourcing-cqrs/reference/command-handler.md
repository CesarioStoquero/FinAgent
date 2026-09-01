# Template canônico — Command + Handler (write side, partida dobrada)

A ordem load → reidrata → regra → append → publish → clear é FIXA. Ver invariantes 7 e 8.
O agregado é a `Account` (ADR-0008); o handler continua tocando UM agregado — os dois lados
da partida dobrada moram DENTRO do único evento `TransactionPosted`, então o append é único
e atômico.

## Contratos CQRS (mínimos, sem framework)

```csharp
namespace FinAgent.Wallet.Application;

public interface ICommand<TResult>;
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct);
}
```

## Portas (definidas no Domain, implementadas na Infra)

```csharp
namespace FinAgent.Wallet.Domain.Ports;

public interface IEventStore
{
    Task<IReadOnlyList<IDomainEvent>> LoadAsync(Guid aggregateId, CancellationToken ct);
    // Lança ConcurrencyException se a versão no banco != expectedVersion.
    Task AppendAsync(Guid aggregateId, IReadOnlyList<IDomainEvent> events, int expectedVersion, CancellationToken ct);
}

public interface IEventPublisher
{
    Task PublishAsync(IReadOnlyList<IDomainEvent> events, CancellationToken ct);
}
```

## Handler (orquestra; regra de negócio e soma-zero estão no agregado)

```csharp
namespace FinAgent.Wallet.Application.Withdraw;

public sealed record WithdrawCommand(Guid AccountId, long AmountCents) : ICommand<Unit>;

public sealed class WithdrawHandler(IEventStore store, IEventPublisher publisher)
    : ICommandHandler<WithdrawCommand, Unit>
{
    public async Task<Unit> HandleAsync(WithdrawCommand cmd, CancellationToken ct)
    {
        var history = await store.LoadAsync(cmd.AccountId, ct);          // 1
        if (history.Count == 0) throw new DomainException("Conta inexistente.");

        var account = Account.Rehydrate(history);                        // 2
        account.Withdraw(new Money(cmd.AmountCents, account.Balance.Currency)); // 3 (regra + soma-zero no agg)

        // 4 — um único append; TransactionPosted já carrega os dois lados balanceados.
        await store.AppendAsync(cmd.AccountId, account.UncommittedEvents, account.Version, ct);
        await publisher.PublishAsync(account.UncommittedEvents, ct);      // 5
        account.ClearUncommittedEvents();                                // 6
        return Unit.Value;
    }
}
```

## Erros que a IA comete aqui (evite)

- Montar os dois lados da partida dobrada no handler — a soma zero (RN-1) é responsabilidade
  do agregado (`EnsureBalanced`). Handler só orquestra.
- Tentar dar `AppendAsync` em duas contas separadas para "debitar uma e creditar outra" —
  no modelo do ADR-0008 os dois lados vêm num só evento, um só append. Nada de dois appends.
- Pular `expectedVersion`, publicar antes do append, ou esquecer o `ClearUncommittedEvents`.
- Retornar o saldo direto da escrita — quem lê saldo é a query (read model).
