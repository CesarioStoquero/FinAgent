using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using FinAgent.Wallet.Application.OpenWallet;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Api;

/// <summary>
/// T22 — <c>POST /wallets</c> com <see cref="WalletApiFactory"/>
/// (<c>Wallet:Store=InMemory</c>, plano D5): roda de verdade, sem Docker.
/// </summary>
public class AbrirCarteiraEndpointTests(WalletApiFactory factory) : IClassFixture<WalletApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Post_Wallets_ComTitularNovo_Retorna201ComWalletIdEMoedaBrl()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();
        var ownerId = Guid.NewGuid();
        client.DefaultRequestHeaders.Add("X-Owner-Id", ownerId.ToString());

        var response = await client.PostAsync("/wallets", null, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<OpenWalletResult>(JsonOptions, ct);
        body.Should().NotBeNull();
        body!.OwnerId.Should().Be(ownerId);
        body.Currency.Should().Be("BRL");
        body.AlreadyExisted.Should().BeFalse();
    }

    [Fact]
    public async Task Post_Wallets_RepetidoParaOMesmoTitular_Retorna200ComOMesmoWalletId()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();
        var ownerId = Guid.NewGuid();
        client.DefaultRequestHeaders.Add("X-Owner-Id", ownerId.ToString());

        var first = await client.PostAsync("/wallets", null, ct);
        var firstBody = await first.Content.ReadFromJsonAsync<OpenWalletResult>(JsonOptions, ct);

        var second = await client.PostAsync("/wallets", null, ct);

        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondBody = await second.Content.ReadFromJsonAsync<OpenWalletResult>(JsonOptions, ct);
        secondBody.Should().NotBeNull();
        secondBody!.WalletId.Should().Be(firstBody!.WalletId);
        secondBody.AlreadyExisted.Should().BeTrue();
    }

    [Fact]
    public async Task Post_Wallets_SemTitularIdentificado_Retorna401()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();

        var response = await client.PostAsync("/wallets", null, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Wallets_ComOwnerIdGuidEmpty_Retorna401()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Owner-Id", Guid.Empty.ToString());

        var response = await client.PostAsync("/wallets", null, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Wallets_ComOwnerIdNoCorpo_IgnoraOCorpoEUsaOContexto()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();
        var ownerIdDoContexto = Guid.NewGuid();
        var ownerIdNoCorpo = Guid.NewGuid();
        client.DefaultRequestHeaders.Add("X-Owner-Id", ownerIdDoContexto.ToString());

        // RN-14: mesmo que o corpo tente informar outro titular, ele é ignorado —
        // o endpoint nem declara um parâmetro de corpo.
        var response = await client.PostAsJsonAsync("/wallets", new { ownerId = ownerIdNoCorpo }, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<OpenWalletResult>(JsonOptions, ct);
        body.Should().NotBeNull();
        body!.OwnerId.Should().Be(ownerIdDoContexto);
        body.OwnerId.Should().NotBe(ownerIdNoCorpo);
    }
}
