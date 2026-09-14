namespace FinAgent.Wallet.Domain;

/// <summary>
/// A transação inteira em UM evento: dois lados que somam zero (partida dobrada,
/// RN-1, ADR-0008). Não é postado por esta feature (abertura não move dinheiro) —
/// existe porque o vocabulário do ledger e o <c>Apply</c> do agregado precisam dele
/// desde já para depósito/saque (BL-1/BL-6) não quebrarem o replay.
/// </summary>
public sealed record TransactionPosted(
    Guid TransactionId,
    IReadOnlyList<LedgerEntry> Entries,
    string Reason) : IDomainEvent;
