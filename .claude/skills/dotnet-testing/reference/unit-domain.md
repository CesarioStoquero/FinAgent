# Template canônico — Teste de unidade (domínio + handler, partida dobrada)

Sem banco. xUnit + FluentAssertions. Modelo do ADR-0008: agregado `Account`, evento
`TransactionPosted` com lançamentos que somam zero.

## Agregado — testar via eventos e a soma zero

```csharp
public class AccountTests
{
    private static Account ContaComSaldo(long cents, string moeda = "BRL")
    {
        var id = Guid.NewGuid();
        // Arrange do estado via HISTÓRICO: abre a conta e credita (depósito).
        return Account.Rehydrate(new IDomainEvent[]
        {
            new AccountOpened(id, moeda),
            new TransactionPosted(Guid.NewGuid(), new[]
            {
                new LedgerEntry(id, EntryDirection.Credit, cents),
                new LedgerEntry(SystemAccounts.ExternalFunding, EntryDirection.Debit, cents),
            }, "deposit"),
        });
    }

    [Fact]
    public void Withdraw_ComSaldoSuficiente_PostaTransacaoBalanceada()
    {
        var account = ContaComSaldo(10_000); // R$100,00

        account.Withdraw(new Money(3_000, "BRL"));

        var tx = account.UncommittedEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TransactionPosted>().Subject;
        // RN-1: débitos == créditos
        tx.Entries.Where(e => e.Direction == EntryDirection.Debit).Sum(e => e.AmountCents)
            .Should().Be(tx.Entries.Where(e => e.Direction == EntryDirection.Credit).Sum(e => e.AmountCents));
        account.Balance.Cents.Should().Be(7_000);
    }

    [Fact]
    public void Withdraw_ComSaldoInsuficiente_LancaDomainException() // RN-10
    {
        var account = ContaComSaldo(1_000);

        var act = () => account.Withdraw(new Money(5_000, "BRL"));

        act.Should().Throw<DomainException>().WithMessage("*insuficiente*");
        account.UncommittedEvents.Should().BeEmpty(); // falhou ANTES de postar
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Withdraw_ComValorNaoPositivo_LancaDomainException(long cents) // RN-11
    {
        var account = ContaComSaldo(10_000);
        var act = () => account.Withdraw(new Money(cents, "BRL"));
        act.Should().Throw<DomainException>();
    }
}
```

## Handler — portas mockadas (NSubstitute)

```csharp
public class WithdrawHandlerTests
{
    private readonly IEventStore _store = Substitute.For<IEventStore>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();

    [Fact]
    public async Task Handle_CaminhoFeliz_FazAppendUnicoComVersaoEPublica()
    {
        var id = Guid.NewGuid();
        _store.LoadAsync(id, Arg.Any<CancellationToken>()).Returns(new IDomainEvent[]
        {
            new AccountOpened(id, "BRL"),
            new TransactionPosted(Guid.NewGuid(), new[]
            {
                new LedgerEntry(id, EntryDirection.Credit, 10_000),
                new LedgerEntry(SystemAccounts.ExternalFunding, EntryDirection.Debit, 10_000),
            }, "deposit"),
        });
        var handler = new WithdrawHandler(_store, _publisher);

        await handler.HandleAsync(new WithdrawCommand(id, 3_000), default);

        await _store.Received(1).AppendAsync(id, Arg.Any<IReadOnlyList<IDomainEvent>>(),
            Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ContaInexistente_LancaDomainExceptionENaoPersiste()
    {
        _store.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<IDomainEvent>());
        var handler = new WithdrawHandler(_store, _publisher);

        var act = () => handler.HandleAsync(new WithdrawCommand(Guid.NewGuid(), 100), default);

        await act.Should().ThrowAsync<DomainException>();
        await _store.DidNotReceive().AppendAsync(Arg.Any<Guid>(),
            Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
```
