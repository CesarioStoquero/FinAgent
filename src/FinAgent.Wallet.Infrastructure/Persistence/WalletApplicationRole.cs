namespace FinAgent.Wallet.Infrastructure.Persistence;

/// <summary>
/// Identidade da role restrita que sustenta RN-3 (o passado é fisicamente imutável):
/// só SELECT/INSERT em <c>wallet_events</c>; UPDATE/DELETE são revogados na migração
/// inicial (<c>Migrations/*_InitialCreate.cs</c>, ADR-0011 seção 5). Estas constantes
/// existem para os testes de integração autenticarem como essa role — o valor em si
/// só é criado/gerido pela migração, nunca por código de produção em runtime.
/// </summary>
internal static class WalletApplicationRole
{
    public const string Name = "wallet_app";
    public const string Password = "wallet_app";
}
