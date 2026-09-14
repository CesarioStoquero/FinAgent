using AwesomeAssertions;
using FinAgent.Wallet.Domain;
using Xunit;

namespace FinAgent.Wallet.UnitTests.Domain;

public class WalletIdentityTests
{
    [Fact]
    public void ForOwner_ChamadaDuasVezes_DevolveOMesmoId()
    {
        var ownerId = Guid.NewGuid();

        var first = WalletIdentity.ForOwner(ownerId);
        var second = WalletIdentity.ForOwner(ownerId);

        first.Should().Be(second);
    }

    [Fact]
    public void ForOwner_TitularesDiferentes_DevolvemIdsDiferentes()
    {
        var a = WalletIdentity.ForOwner(Guid.NewGuid());
        var b = WalletIdentity.ForOwner(Guid.NewGuid());

        a.Should().NotBe(b);
    }

    [Fact]
    public void ForOwner_ComTitularConhecido_DevolveOValorAncora()
    {
        // Golden value: trava a derivação. Mudar o algoritmo deixaria carteiras
        // existentes órfãs — se este teste quebrar, é sinal de regressão grave.
        var ownerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var walletId = WalletIdentity.ForOwner(ownerId);

        walletId.Should().Be(Guid.Parse("9c5c21c1-e086-e05d-8e96-6ecfa84914a3"));
    }
}
