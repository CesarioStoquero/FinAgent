using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Domain.Ports;

namespace FinAgent.Wallet.Application.OpenWallet;

/// <summary>
/// Abre a carteira do titular. RF-4/RN-15 (no máximo uma por titular) é resolvido
/// estruturalmente pelo id derivado (<see cref="WalletIdentity.ForOwner"/>, plano D2):
/// o segundo pedido cai no mesmo stream e devolve a carteira existente sem novo append.
/// </summary>
public sealed class OpenWalletHandler(IEventStore store, IEventPublisher publisher)
    : ICommandHandler<OpenWalletCommand, OpenWalletResult>
{
    public async Task<OpenWalletResult> HandleAsync(OpenWalletCommand command, CancellationToken ct)
    {
        var walletId = WalletIdentity.ForOwner(command.OwnerId);          // 1
        var history = await store.LoadAsync(walletId, ct);                // 2

        if (history.Count > 0)
        {
            // RF-4/RN-15: titular já tem carteira — devolve a existente, sem append.
            var existing = Account.Rehydrate(history);
            return new OpenWalletResult(existing.Id, command.OwnerId, existing.Balance.Currency, AlreadyExisted: true);
        }

        var account = Account.Open(command.OwnerId, Currencies.Brl);      // 3 (regra no agregado)

        try
        {
            var envelopes = await store.AppendAsync(walletId, account.UncommittedEvents, account.Version, ct); // 4
            await publisher.PublishAsync(envelopes, ct);                  // 5
            account.ClearUncommittedEvents();                             // 6
        }
        catch (ConcurrencyException)
        {
            // Corrida (plano D2): outra abertura concorrente do mesmo titular venceu.
            // Recarrega e devolve a carteira existente.
            var afterRace = await store.LoadAsync(walletId, ct);
            var existing = Account.Rehydrate(afterRace);
            return new OpenWalletResult(existing.Id, command.OwnerId, existing.Balance.Currency, AlreadyExisted: true);
        }

        return new OpenWalletResult(account.Id, command.OwnerId, account.Balance.Currency, AlreadyExisted: false);
    }
}
