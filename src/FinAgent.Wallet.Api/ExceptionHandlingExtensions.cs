namespace FinAgent.Wallet.Api;

/// <summary>
/// Registro do mapeamento de erro (T23). A Api compõe este método junto com
/// <c>AddWalletApplication</c>/<c>AddWalletInfrastructure</c> e ativa o pipeline com
/// <c>app.UseExceptionHandler()</c>.
/// </summary>
internal static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddDomainExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }
}
