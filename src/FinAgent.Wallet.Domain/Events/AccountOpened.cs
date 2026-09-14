namespace FinAgent.Wallet.Domain;

/// <summary>
/// Fato: a carteira do titular foi aberta. Carrega SÓ ids opacos, moeda e tipo —
/// nenhum dado pessoal (RF-10).
/// </summary>
public sealed record AccountOpened(Guid AccountId, Guid OwnerId, string Currency, AccountType Type) : IDomainEvent;
