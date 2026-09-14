namespace FinAgent.Wallet.Domain;

/// <summary>
/// Um lado de uma transação: uma conta é debitada OU creditada em um valor.
/// </summary>
public sealed record LedgerEntry(Guid AccountId, EntryDirection Direction, long AmountCents);
