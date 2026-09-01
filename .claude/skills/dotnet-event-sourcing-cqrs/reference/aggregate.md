# Template canônico — Agregado Account, Lançamentos e Money (partida dobrada)

Modelo de partida dobrada (ADR-0008). O agregado é a `Account`; a carteira do usuário é
uma `Account` do tipo `Customer`. Toda movimentação é UM evento `TransactionPosted` com
lançamentos que somam zero. Nomes de método (`Raise`, `Apply`, `Rehydrate`,
`UncommittedEvents`, `ClearUncommittedEvents`, `Version`) são o CONTRATO — não renomeie.

## Money (centavos) — value object

```csharp
namespace FinAgent.Wallet.Domain;

public readonly record struct Money(long Cents, string Currency)
{
    public static Money Zero(string currency) => new(0, currency);
    public Money Add(Money o)      { EnsureSame(o); return this with { Cents = Cents + o.Cents }; }
    public Money Subtract(Money o) { EnsureSame(o); return this with { Cents = Cents - o.Cents }; }
    private void EnsureSame(Money o)
    {
        if (Currency != o.Currency)
            throw new DomainException($"Moedas divergentes: {Currency} vs {o.Currency}."); // RN-9
    }
}
```

## Lançamento (LedgerEntry) e evento balanceado

```csharp
namespace FinAgent.Wallet.Domain;

public enum EntryDirection { Debit, Credit }

// Um lado de uma transação: uma conta é debitada OU creditada em um valor.
public sealed record LedgerEntry(Guid AccountId, EntryDirection Direction, long AmountCents);

public interface IDomainEvent;

public sealed record AccountOpened(Guid AccountId, string Currency) : IDomainEvent;

// A transação inteira em UM evento: dois lados que somam zero (partida dobrada, RN-1).
public sealed record TransactionPosted(
    Guid TransactionId,
    IReadOnlyList<LedgerEntry> Entries,
    string Reason) : IDomainEvent;
```

## Conta de contrapartida do sistema (ADR-0008)

```csharp
namespace FinAgent.Wallet.Domain;

// Conta virtual do "mundo externo": de onde o dinheiro entra / para onde sai.
// Sem regra de saldo; existe só como o outro lado dos lançamentos.
public static class SystemAccounts
{
    public static readonly Guid ExternalFunding = Guid.Parse("00000000-0000-0000-0000-0000000000f1");
}
```

## Base de agregado (igual ao restante do projeto)

```csharp
namespace FinAgent.Wallet.Domain;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _uncommitted = [];
    public Guid Id { get; protected set; }
    public int Version { get; private set; } = -1;
    public IReadOnlyList<IDomainEvent> UncommittedEvents => _uncommitted;
    public void ClearUncommittedEvents() => _uncommitted.Clear();
    protected void Raise(IDomainEvent e) { Apply(e); _uncommitted.Add(e); }
    protected void Replay(IReadOnlyList<IDomainEvent> history)
    { foreach (var e in history) { Apply(e); Version++; } }
    protected abstract void Apply(IDomainEvent e);
}
```

## Agregado Account (regra de negócio vive AQUI)

```csharp
namespace FinAgent.Wallet.Domain;

public sealed class Account : AggregateRoot
{
    private string _currency = "";
    public Money Balance { get; private set; }

    private Account() { }

    public static Account Open(Guid id, string currency)
    {
        var a = new Account();
        a.Raise(new AccountOpened(id, currency));
        return a;
    }

    public static Account Rehydrate(IReadOnlyList<IDomainEvent> history)
    {
        var a = new Account();
        a.Replay(history);
        return a;
    }

    // Depósito: credita esta conta e debita a conta externa (soma zero).
    public void Deposit(Money amount)
    {
        if (amount.Cents <= 0) throw new DomainException("Depósito deve ser positivo."); // RN-11
        Post("deposit", amount,
            credit: Id,
            debit: SystemAccounts.ExternalFunding);
    }

    // Saque: debita esta conta e credita a conta externa. Valida saldo (RN-10).
    public void Withdraw(Money amount)
    {
        if (amount.Cents <= 0) throw new DomainException("Saque deve ser positivo."); // RN-11
        if (amount.Currency != _currency) throw new DomainException("Moeda divergente."); // RN-9
        if (amount.Cents > Balance.Cents) throw new DomainException("Saldo insuficiente."); // RN-10
        Post("withdraw", amount,
            debit: Id,
            credit: SystemAccounts.ExternalFunding);
    }

    private void Post(string reason, Money amount, Guid debit, Guid credit)
    {
        var entries = new[]
        {
            new LedgerEntry(debit,  EntryDirection.Debit,  amount.Cents),
            new LedgerEntry(credit, EntryDirection.Credit, amount.Cents),
        };
        EnsureBalanced(entries); // RN-1: soma zero, sempre
        Raise(new TransactionPosted(Guid.NewGuid(), entries, reason));
    }

    // RN-1: total de débitos == total de créditos. Não fecha -> não posta.
    private static void EnsureBalanced(IReadOnlyList<LedgerEntry> entries)
    {
        long debits  = entries.Where(e => e.Direction == EntryDirection.Debit ).Sum(e => e.AmountCents);
        long credits = entries.Where(e => e.Direction == EntryDirection.Credit).Sum(e => e.AmountCents);
        if (debits != credits)
            throw new DomainException($"Transação desbalanceada: débitos={debits}, créditos={credits}.");
    }

    // Um case por evento. SÓ muda estado — o saldo reflete só os lançamentos DESTA conta.
    protected override void Apply(IDomainEvent e)
    {
        switch (e)
        {
            case AccountOpened ev:
                Id = ev.AccountId; _currency = ev.Currency; Balance = Money.Zero(ev.Currency);
                break;
            case TransactionPosted ev:
                foreach (var entry in ev.Entries.Where(x => x.AccountId == Id))
                {
                    var delta = new Money(entry.AmountCents, _currency);
                    Balance = entry.Direction == EntryDirection.Credit
                        ? Balance.Add(delta)      // crédito aumenta o saldo do titular
                        : Balance.Subtract(delta); // débito diminui
                }
                break;
        }
    }
}
```

## Estorno (RN-4) — correção sem tocar o passado

Para corrigir uma transação postada, NÃO edite: poste uma nova invertendo os lados.
Modele um método `Reverse(originalTransactionId, entries)` que cria um `TransactionPosted`
com `Debit`↔`Credit` trocados. O histórico fica auditável (RN-3, RN-6).

## Erros que a IA comete aqui (evite)

- Postar só um lado (só debitar a carteira) — quebra a soma zero (RN-1). Todo movimento
  tem os dois lados no MESMO evento.
- Validar dentro do `Apply` — `Apply` reexecuta no replay e não pode falhar. Toda validação
  (saldo, valor, moeda) vai ANTES do `Raise`, nos métodos `Deposit`/`Withdraw`.
- Dar saldo à conta `ExternalFunding` como se fosse regra — ela é contrapartida virtual,
  saldo só na projeção (ADR-0008).
- `Money` com `decimal`/`double` — proibido. Sempre `long` de centavos.
