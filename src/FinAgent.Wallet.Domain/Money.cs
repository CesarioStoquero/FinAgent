namespace FinAgent.Wallet.Domain;

/// <summary>
/// Dinheiro em centavos (RN-8). Proibido <c>decimal</c>/<c>double</c>/<c>float</c>
/// em valor monetário.
/// </summary>
public readonly record struct Money(long Cents, string Currency)
{
    public static Money Zero(string currency) => new(0, currency);

    public Money Add(Money o)
    {
        EnsureSame(o);
        return this with { Cents = Cents + o.Cents };
    }

    public Money Subtract(Money o)
    {
        EnsureSame(o);
        return this with { Cents = Cents - o.Cents };
    }

    private void EnsureSame(Money o)
    {
        if (Currency != o.Currency)
        {
            throw new CurrencyMismatchException(Currency, o.Currency); // RN-9
        }
    }
}

/// <summary>
/// Moedas conhecidas do sistema. A v1 só opera em BRL (RN-9).
/// </summary>
public static class Currencies
{
    public const string Brl = "BRL";
}
