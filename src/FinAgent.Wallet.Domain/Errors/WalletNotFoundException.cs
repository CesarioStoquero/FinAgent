namespace FinAgent.Wallet.Domain;

/// <summary>
/// A operação foi dirigida a uma carteira que nunca foi aberta (RN-12). O motivo
/// é um texto estável, consumido pelo mapeamento de ProblemDetails na borda.
/// </summary>
public sealed class WalletNotFoundException(Guid walletId)
    : DomainException($"carteira inexistente: {walletId}")
{
    public Guid WalletId { get; } = walletId;
}
