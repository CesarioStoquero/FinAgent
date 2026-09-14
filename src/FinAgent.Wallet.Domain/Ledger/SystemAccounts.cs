namespace FinAgent.Wallet.Domain;

/// <summary>
/// Conta virtual do "mundo externo": de onde o dinheiro entra / para onde sai.
/// Sem regra de saldo; existe só como o outro lado dos lançamentos (ADR-0008).
/// </summary>
public static class SystemAccounts
{
    public static readonly Guid ExternalFunding = Guid.Parse("00000000-0000-0000-0000-0000000000f1");
}
