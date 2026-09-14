using AwesomeAssertions;
using FinAgent.Wallet.Domain;
using Xunit;

namespace FinAgent.Wallet.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Zero_EmBrl_TemZeroCentavosEMoedaBrl()
    {
        var money = Money.Zero(Currencies.Brl);

        money.Cents.Should().Be(0);
        money.Currency.Should().Be(Currencies.Brl);
    }

    [Fact]
    public void Add_ComMoedaDivergente_LancaCurrencyMismatchException()
    {
        var brl = new Money(1_000, Currencies.Brl);
        var usd = new Money(1_000, "USD");

        var act = () => brl.Add(usd);

        act.Should().Throw<CurrencyMismatchException>()
            .Which.Message.Should().Contain("moeda divergente");
    }

    [Fact]
    public void Add_MesmaMoeda_SomaOsCentavos()
    {
        var a = new Money(1_000, Currencies.Brl);
        var b = new Money(500, Currencies.Brl);

        var result = a.Add(b);

        result.Cents.Should().Be(1_500);
        result.Currency.Should().Be(Currencies.Brl);
    }
}
