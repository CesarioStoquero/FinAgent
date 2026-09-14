using AwesomeAssertions;
using FinAgent.Wallet.Domain;
using Xunit;

namespace FinAgent.Wallet.UnitTests.Domain;

public class AccountTests
{
    [Fact]
    public void Open_ComTitularEMoedaBrl_LevantaUmAccountOpened()
    {
        var ownerId = Guid.NewGuid();

        var account = Account.Open(ownerId, Currencies.Brl);

        var opened = account.UncommittedEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AccountOpened>().Subject;
        opened.OwnerId.Should().Be(ownerId);
        opened.Currency.Should().Be(Currencies.Brl);
        opened.Type.Should().Be(AccountType.Customer);
    }

    [Fact]
    public void Open_RecemAberta_TemSaldoZeroEmBrl()
    {
        var account = Account.Open(Guid.NewGuid(), Currencies.Brl);

        account.Balance.Should().Be(Money.Zero(Currencies.Brl));
    }

    [Fact]
    public void Rehydrate_ComAccountOpened_ReconstroiSaldoZeroEVersaoZero()
    {
        var accountId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var history = new IDomainEvent[]
        {
            new AccountOpened(accountId, ownerId, Currencies.Brl, AccountType.Customer),
        };

        var account = Account.Rehydrate(history);

        account.Id.Should().Be(accountId);
        account.Version.Should().Be(0);
        account.Balance.Should().Be(Money.Zero(Currencies.Brl));
    }

    [Fact]
    public void AccountOpened_NaoCarregaDadoPessoal()
    {
        var evento = new AccountOpened(Guid.NewGuid(), Guid.NewGuid(), Currencies.Brl, AccountType.Customer);

        // RF-10: só ids opacos, moeda e tipo — nenhum nome/CPF/e-mail/telefone.
        var propriedades = typeof(AccountOpened).GetProperties().Select(p => p.Name);
        propriedades.Should().BeEquivalentTo(["AccountId", "OwnerId", "Currency", "Type"]);
    }
}
