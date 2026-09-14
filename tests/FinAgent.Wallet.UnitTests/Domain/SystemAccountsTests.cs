using AwesomeAssertions;
using FinAgent.Wallet.Domain;
using Xunit;

namespace FinAgent.Wallet.UnitTests.Domain;

public class SystemAccountsTests
{
    [Fact]
    public void ExternalFunding_EhUmGuidConstante_NaoMudaEntreExecucoes()
    {
        // Guarda contra alteração acidental que quebraria projeções futuras.
        SystemAccounts.ExternalFunding.Should().Be(Guid.Parse("00000000-0000-0000-0000-0000000000f1"));
    }
}
