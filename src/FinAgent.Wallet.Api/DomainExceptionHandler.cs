using FinAgent.Wallet.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinAgent.Wallet.Api;

/// <summary>
/// Mapeamento único de exceção de domínio para ProblemDetails (T23). Nenhum stack
/// trace nem dado sensível no corpo da resposta (LGPD-9) — só status e motivo, o
/// vocabulário estável que BL-1, BL-2, BL-3, BL-6 e BL-7 vão reutilizar.
/// </summary>
internal sealed class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            // Mais específicos primeiro: todos herdam de DomainException.
            WalletNotFoundException => (StatusCodes.Status404NotFound, "carteira inexistente"),       // RN-12
            ConcurrencyException => (StatusCodes.Status409Conflict, "conflito de concorrência"),       // ADR-0007
            CurrencyMismatchException => (StatusCodes.Status422UnprocessableEntity, "moeda divergente"), // RN-9
            // Genérico: título fixo, NUNCA exception.Message — texto livre de domínio
            // não pode virar contrato de API por acidente.
            DomainException => (StatusCodes.Status422UnprocessableEntity, "regra de negócio violada"),
            _ => (0, string.Empty),
        };

        if (status == 0)
        {
            // Não é um erro de domínio conhecido — deixa o pipeline padrão tratar.
            return false;
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails { Status = status, Title = title },
            cancellationToken);

        return true;
    }
}
