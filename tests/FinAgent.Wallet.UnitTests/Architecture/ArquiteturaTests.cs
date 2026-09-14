using System.Reflection;
using AwesomeAssertions;
using FinAgent.Wallet.Application.OpenWallet;
using FinAgent.Wallet.Domain;
using Xunit;

namespace FinAgent.Wallet.UnitTests.Architecture;

/// <summary>
/// Verifica por máquina a regra de dependência do ADR-0003: dependências apontam
/// para dentro. A partir daqui, qualquer violação futura quebra a build.
/// </summary>
public class ArquiteturaTests
{
    private static readonly string[] InfraAssemblyPrefixes =
    [
        "Npgsql", "MongoDB.Driver", "Confluent.Kafka", "Microsoft.EntityFrameworkCore",
    ];

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "FinAgent.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("FinAgent.slnx não encontrado.");
    }

    [Fact]
    public void Domain_NaoReferenciaNenhumOutroProjeto()
    {
        var root = FindSolutionRoot();
        var csprojPath = Path.Combine(root, "src", "FinAgent.Wallet.Domain", "FinAgent.Wallet.Domain.csproj");
        var content = File.ReadAllText(csprojPath);

        content.Should().NotContain("<ProjectReference");
    }

    [Fact]
    public void Domain_NaoReferenciaAssemblyDeInfra()
    {
        var domainAssembly = typeof(Money).Assembly;

        var referencedNames = domainAssembly.GetReferencedAssemblies().Select(a => a.Name ?? "");

        referencedNames.Should().NotContain(name => InfraAssemblyPrefixes.Any(name.StartsWith));
    }

    [Fact]
    public void Application_NaoReferenciaInfrastructure()
    {
        var applicationAssembly = typeof(OpenWalletCommand).Assembly;

        var referencedNames = applicationAssembly.GetReferencedAssemblies().Select(a => a.Name ?? "");

        referencedNames.Should().NotContain("FinAgent.Wallet.Infrastructure");
    }

    [Fact]
    public void Dominio_NaoUsaTipoDecimalOuDoubleEmValorMonetario()
    {
        var proibidos = new[] { typeof(decimal), typeof(double), typeof(float) };
        var domainAssembly = typeof(Money).Assembly;

        var propriedadesProibidas = domainAssembly.GetTypes()
            .Where(t => t.IsPublic)
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(p => proibidos.Contains(p.PropertyType))
            .ToList();

        propriedadesProibidas.Should().BeEmpty();
    }
}
