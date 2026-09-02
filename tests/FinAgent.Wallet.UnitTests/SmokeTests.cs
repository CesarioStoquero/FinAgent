using Xunit;
using AwesomeAssertions;

namespace FinAgent.Wallet.UnitTests;

/// <summary>
/// Teste de fumaça do bootstrap (BL-4): só prova que o harness de teste roda —
/// xUnit v3 descobre o teste e AwesomeAssertions funciona. Nenhuma regra de
/// negócio é exercitada aqui.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void Harness_DeTeste_EstaFuncionando()
    {
        var resultado = 1;

        resultado.Should().Be(1);
    }
}
