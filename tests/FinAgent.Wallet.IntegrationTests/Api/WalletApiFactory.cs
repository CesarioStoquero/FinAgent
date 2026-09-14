using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FinAgent.Wallet.IntegrationTests.Api;

/// <summary>
/// Sobe a Wallet Api de ponta a ponta com <c>Wallet:Store=InMemory</c> (plano D5) —
/// sem Docker, sem Testcontainers. Usado pelos testes de T22/T23.
/// </summary>
public sealed class WalletApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Wallet:Store", "InMemory");
    }
}
