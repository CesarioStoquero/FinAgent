using AwesomeAssertions;
using FinAgent.Wallet.Domain;
using Xunit;

namespace FinAgent.Wallet.UnitTests.Domain;

public class DomainErrorsTests
{
    [Fact]
    public void WalletNotFound_TemMotivoCarteiraInexistente()
    {
        var error = new WalletNotFoundException(Guid.NewGuid());

        error.Message.Should().Contain("carteira inexistente");
    }

    [Fact]
    public void WalletNotFound_EhUmDomainException()
    {
        var error = new WalletNotFoundException(Guid.NewGuid());

        error.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void CurrencyMismatch_TemMotivoEstavelMoedaDivergente()
    {
        var error = new CurrencyMismatchException("BRL", "USD");

        error.Message.Should().Be("moeda divergente");
        error.Should().BeAssignableTo<DomainException>();
    }
}
