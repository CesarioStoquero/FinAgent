namespace FinAgent.Wallet.Application;

/// <summary>
/// Retorno de comando que não produz valor útil (equivalente a <c>void</c>, mas
/// utilizável como <c>TResult</c> genérico).
/// </summary>
public readonly record struct Unit
{
    public static readonly Unit Value = default;
}
