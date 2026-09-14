using Microsoft.EntityFrameworkCore;

namespace FinAgent.Wallet.Infrastructure.Persistence;

/// <summary>
/// Aplica as migrações pendentes do event store (ADR-0011 seção 4). O caminho canônico
/// continua sendo <c>dotnet ef database update</c>; isto cobre as duas conveniências que a
/// Api oferece: aplicar na subida (Development, opt-in por configuração) e o `--migrate`
/// explícito (aplica e encerra sem subir o servidor).
/// </summary>
internal static class MigrationRunner
{
    public static Task ApplyAsync(WalletEventStoreDbContext context, CancellationToken ct = default) =>
        context.Database.MigrateAsync(ct);
}
