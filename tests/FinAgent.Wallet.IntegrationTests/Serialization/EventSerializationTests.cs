using AwesomeAssertions;
using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Infrastructure.Serialization;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Serialization;

/// <summary>
/// T14 — não precisa de container: só exercita serialização em memória. Mora em
/// IntegrationTests porque é o único projeto que referencia a Infrastructure.
/// </summary>
public class EventSerializationTests
{
    [Fact]
    public void Serializar_AccountOpened_UsaONomeDeContratoEstavel()
    {
        var accountId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var evento = new AccountOpened(accountId, ownerId, Currencies.Brl, AccountType.Customer);

        var (eventType, _) = JsonEventSerializer.Serialize(evento);

        eventType.Should().Be("wallet.account-opened");
    }

    [Fact]
    public void RoundTrip_AccountOpened_PreservaTodosOsCampos()
    {
        var accountId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var original = new AccountOpened(accountId, ownerId, Currencies.Brl, AccountType.Customer);

        var (eventType, payload) = JsonEventSerializer.Serialize(original);
        var deserialized = JsonEventSerializer.Deserialize(eventType, payload);

        deserialized.Should().BeOfType<AccountOpened>().Which.Should().Be(original);
    }

    [Fact]
    public void Desserializar_ComNomeDeContratoDesconhecido_Lanca()
    {
        var act = () => JsonEventSerializer.Deserialize("wallet.nunca-existiu", "{}");

        act.Should().Throw<InvalidOperationException>();
    }
}
