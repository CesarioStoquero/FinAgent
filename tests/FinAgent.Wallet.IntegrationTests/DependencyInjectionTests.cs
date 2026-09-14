using AwesomeAssertions;
using FinAgent.Wallet.Application;
using FinAgent.Wallet.Application.OpenWallet;
using FinAgent.Wallet.Domain.Ports;
using FinAgent.Wallet.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests;

/// <summary>
/// T21 — sem container: só monta um <see cref="ServiceCollection"/> real e resolve.
/// Prova a composição de DI (plano D5), não infraestrutura de verdade.
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public void AddWalletInfrastructure_ComStoreInMemory_ResolveTodasAsPortas()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Wallet:Store"] = "InMemory" })
            .Build();

        var services = new ServiceCollection();
        services.AddWalletInfrastructure(configuration);
        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IEventStore>().Should().NotBeNull();
        provider.GetRequiredService<IEventPublisher>().Should().NotBeNull();
        provider.GetRequiredService<IWalletReadModel>().Should().NotBeNull();
    }

    [Fact]
    public void AddWalletApplication_ResolveOOpenWalletHandler()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Wallet:Store"] = "InMemory" })
            .Build();

        var services = new ServiceCollection();
        services.AddWalletApplication();
        services.AddWalletInfrastructure(configuration); // OpenWalletHandler depende das portas
        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<ICommandHandler<OpenWalletCommand, OpenWalletResult>>().Should().NotBeNull();
    }
}
