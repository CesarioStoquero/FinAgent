namespace FinAgent.Wallet.Domain;

/// <summary>
/// A operação envolveu duas moedas diferentes (RN-9). O motivo é um texto estável
/// ("moeda divergente"), consumido pelo mapeamento de ProblemDetails na borda —
/// vocabulário que BL-1, BL-6 e BL-7 vão reutilizar.
/// </summary>
public sealed class CurrencyMismatchException(string expected, string actual)
    : DomainException("moeda divergente")
{
    public string Expected { get; } = expected;

    public string Actual { get; } = actual;
}
