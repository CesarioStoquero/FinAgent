using System.Net;
using AwesomeAssertions;
using FinAgent.Wallet.Api;
using FinAgent.Wallet.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Api;

/// <summary>
/// T23 — mapeamento único de exceção de domínio para ProblemDetails (RN-12, RN-9).
/// Sobe um host mínimo, isolado do resto da Wallet Api, só com
/// <see cref="DomainExceptionHandler"/> registrado e um endpoint de teste que lança
/// a exceção sob teste — exatamente o que a tarefa pede. Usa <c>HostBuilder</c> +
/// <c>UseTestServer()</c> porque <c>WebHostBuilder</c>/<c>TestServer(IWebHostBuilder)</c>
/// estão obsoletos no .NET 10 (ASPDEPR004/ASPDEPR008).
/// </summary>
public class ProblemDetailsMappingTests
{
    private static IHost CreateHost(Func<Exception> exceptionFactory)
    {
        return new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureServices(services => services.AddDomainExceptionHandling())
                .Configure(app =>
                {
                    app.UseExceptionHandler();
                    app.Run(_ => throw exceptionFactory());
                }))
            .Start();
    }

    [Fact]
    public async Task WalletNotFound_Vira404ComMotivoCarteiraInexistente()
    {
        var ct = TestContext.Current.CancellationToken;
        using var host = CreateHost(() => new WalletNotFoundException(Guid.NewGuid()));
        var client = host.GetTestClient();

        var response = await client.GetAsync("/", ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body.Should().Contain("carteira inexistente");
    }

    [Fact]
    public async Task CurrencyMismatchExceptionDeMoneyAdd_Vira422ComMotivoMoedaDivergente()
    {
        var ct = TestContext.Current.CancellationToken;
        // Provoca a divergência de moeda pelo caminho real de produção (Money.Add ->
        // Money.EnsureSame), em vez de construir a exceção com o texto esperado à mão.
        using var host = CreateHost(() =>
        {
            var brl = new Money(1_000, Currencies.Brl);
            var usd = new Money(1_000, "USD");
            return Record.Exception(() => brl.Add(usd))!;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/", ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body.Should().Contain("moeda divergente");
    }

    [Fact]
    public async Task DomainExceptionGenerica_Vira422ComTituloFixoSemVazarMensagem()
    {
        var ct = TestContext.Current.CancellationToken;
        using var host = CreateHost(() => new DomainException("um texto livre qualquer que não pode virar contrato"));
        var client = host.GetTestClient();

        var response = await client.GetAsync("/", ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body.Should().Contain("regra de negócio violada");
        body.Should().NotContain("um texto livre qualquer que não pode virar contrato");
    }

    [Fact]
    public async Task ConcurrencyException_Vira409()
    {
        var ct = TestContext.Current.CancellationToken;
        using var host = CreateHost(() => new ConcurrencyException(Guid.NewGuid(), expectedVersion: 0));
        var client = host.GetTestClient();

        var response = await client.GetAsync("/", ct);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RespostaDeErro_NaoVazaStackTrace()
    {
        var ct = TestContext.Current.CancellationToken;
        using var host = CreateHost(() => new WalletNotFoundException(Guid.NewGuid()));
        var client = host.GetTestClient();

        var response = await client.GetAsync("/", ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        body.Should().NotContain("StackTrace");
        body.Should().NotContain(".cs:line");
        body.Should().NotContain(nameof(WalletNotFoundException));
    }
}
