namespace FinAgent.Wallet.Application.OpenWallet;

public sealed record OpenWalletCommand(Guid OwnerId) : ICommand<OpenWalletResult>;

// D3: a escrita NÃO devolve saldo (quem lê saldo é a query, BL-2); devolve identidade.
public sealed record OpenWalletResult(Guid WalletId, Guid OwnerId, string Currency, bool AlreadyExisted);
