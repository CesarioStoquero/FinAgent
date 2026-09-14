namespace FinAgent.Wallet.Domain.Ports;

/// <summary>
/// Porta de leitura da carteira (read model, ADR-0005). Consulta de saldo é BL-2 —
/// esta porta existe aqui porque a projeção (Infrastructure) precisa de um caminho
/// de leitura para ser verificada.
/// </summary>
public interface IWalletReadModel
{
    public Task<WalletView?> GetByIdAsync(Guid walletId, CancellationToken ct);
}

// DTO de leitura — plano, feito para a UI/agente. NÃO é o agregado.
public sealed record WalletView(Guid WalletId, Guid OwnerId, string Currency, long BalanceCents);
