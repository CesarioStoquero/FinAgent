using FinAgent.Wallet.Api;
using FinAgent.Wallet.Application;
using FinAgent.Wallet.Application.OpenWallet;
using FinAgent.Wallet.Infrastructure;
using FinAgent.Wallet.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var isInMemoryStore = string.Equals(
    builder.Configuration["Wallet:Store"], "InMemory", StringComparison.OrdinalIgnoreCase);

// ADR-0011 seção 4: fora de Development, migração não roda sozinha na subida.
var applyMigrationsOnStartup = builder.Configuration.GetValue<bool?>("Wallet:Postgres:ApplyMigrationsOnStartup")
    ?? builder.Environment.IsDevelopment();

// `--migrate`: aplica a migração pendente e encerra sem subir o servidor (caminho
// explícito, independente de Wallet:Postgres:ApplyMigrationsOnStartup).
var migrateOnly = args.Contains("--migrate", StringComparer.OrdinalIgnoreCase);

builder.Services.AddOpenApi();
builder.Services.AddDomainExceptionHandling();
builder.Services
    .AddWalletApplication()
    .AddWalletInfrastructure(builder.Configuration);

var app = builder.Build();

// ADR-0011: o schema do event store nasce por migração EF Core, não mais por DDL
// aplicada na subida. O adapter in-memory de dev (Wallet:Store=InMemory, plano D5)
// não tem schema nenhum para migrar.
if (!isInMemoryStore && (migrateOnly || applyMigrationsOnStartup))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<WalletEventStoreDbContext>();
    await MigrationRunner.ApplyAsync(dbContext);
}

if (migrateOnly)
{
    return;
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/wallets", async (
    HttpRequest request,
    ICommandHandler<OpenWalletCommand, OpenWalletResult> handler,
    CancellationToken ct) =>
{
    // RN-14/D10: o ownerId vem SÓ do OwnerContext — nunca do corpo do request.
    if (!OwnerContext.TryResolve(request, out var ownerId))
    {
        return Results.Unauthorized();
    }

    var result = await handler.HandleAsync(new OpenWalletCommand(ownerId), ct);

    // D3: a resposta não devolve saldo — só identidade.
    return result.AlreadyExisted
        ? Results.Ok(result)
        : Results.Created($"/wallets/{result.WalletId}", result);
});

app.Run();

// Exposto para o WebApplicationFactory<Program> dos testes de integração (T22/T23).
public partial class Program;
