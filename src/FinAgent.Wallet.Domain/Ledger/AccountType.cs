namespace FinAgent.Wallet.Domain;

/// <summary>
/// Tipo de conta no ledger de partida dobrada (ADR-0008). A carteira do titular é
/// <see cref="Customer"/>; <see cref="ExternalFunding"/> é a contrapartida virtual
/// do sistema.
/// </summary>
public enum AccountType
{
    Customer,
    ExternalFunding,
}
