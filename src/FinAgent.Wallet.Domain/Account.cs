namespace FinAgent.Wallet.Domain;

/// <summary>
/// A carteira do titular é uma <see cref="Account"/> do tipo <see cref="AccountType.Customer"/>
/// no ledger de partida dobrada (ADR-0008). Nesta feature a conta só nasce (<see cref="Open"/>);
/// nenhum movimento é postado aqui (depósito/saque/estorno são specs próprias).
/// </summary>
public sealed class Account : AggregateRoot
{
    private Guid _ownerId;
    private string _currency = "";

    public Money Balance { get; private set; }

    private Account() { }

    public static Account Open(Guid ownerId, string currency)
    {
        var id = WalletIdentity.ForOwner(ownerId);
        var account = new Account();
        account.Raise(new AccountOpened(id, ownerId, currency, AccountType.Customer));
        return account;
    }

    public static Account Rehydrate(IReadOnlyList<IDomainEvent> history)
    {
        var account = new Account();
        account.Replay(history);
        return account;
    }

    // SÓ muda estado — reexecuta no replay e não pode falhar. Toda validação
    // acontece antes do Raise, nunca aqui.
    protected override void Apply(IDomainEvent e)
    {
        switch (e)
        {
            case AccountOpened ev:
                Id = ev.AccountId;
                _ownerId = ev.OwnerId;
                _currency = ev.Currency;
                Balance = Money.Zero(ev.Currency);
                break;
            case TransactionPosted ev:
                // Nenhum método público posta esta feature (Deposit/Withdraw são
                // specs próprias) — o case existe para o replay de eventos futuros
                // não quebrar (RN-1, ADR-0008).
                foreach (var entry in ev.Entries.Where(x => x.AccountId == Id))
                {
                    var delta = new Money(entry.AmountCents, _currency);
                    Balance = entry.Direction == EntryDirection.Credit
                        ? Balance.Add(delta)
                        : Balance.Subtract(delta);
                }

                break;
        }
    }
}
