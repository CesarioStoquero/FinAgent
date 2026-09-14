using FinAgent.Wallet.Application.OpenWallet;
using Microsoft.Extensions.DependencyInjection;

namespace FinAgent.Wallet.Application;

/// <summary>
/// Registro dos casos de uso da Application (template canônico
/// <c>reference/project-layout.md</c>). A Api compõe este método junto com
/// <c>AddWalletInfrastructure</c>.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddWalletApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<OpenWalletCommand, OpenWalletResult>, OpenWalletHandler>();
        return services;
    }
}
