namespace FinAgent.Wallet.Api;

/// <summary>
/// Resolve o titular da requisição (RN-14): o <c>ownerId</c> NUNCA vem do corpo do
/// request — só daqui, senão seria possível abrir/operar carteira de terceiro.
/// Na v1, sem ADR de autenticação (plano D10, pendência P1 do plano), a fonte é o
/// header <c>X-Owner-Id</c> — solução de v1/dev; trocar por autenticação real
/// (JWT/OIDC) é mudar só esta classe.
/// </summary>
internal static class OwnerContext
{
    private const string HeaderName = "X-Owner-Id";

    public static bool TryResolve(HttpRequest request, out Guid ownerId)
    {
        ownerId = Guid.Empty;

        if (!request.Headers.TryGetValue(HeaderName, out var values)
            || !Guid.TryParse(values.ToString(), out ownerId))
        {
            return false;
        }

        // Guid.Empty não identifica ninguém — trata como titular não identificado (401),
        // em vez de abrir carteira para um "titular nulo".
        return ownerId != Guid.Empty;
    }
}
