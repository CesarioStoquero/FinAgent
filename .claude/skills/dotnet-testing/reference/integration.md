# Template canônico — Teste de integração (Testcontainers)

Container real. NUNCA mocke o banco aqui. NUNCA `Thread.Sleep` — faça poll de condição.

## Fixture com container Postgres (event store)

```csharp
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine").Build();

    public string ConnectionString => _pg.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();
        // aplicar migração/DDL do event store aqui
    }

    public Task DisposeAsync() => _pg.DisposeAsync().AsTask();
}
```

## Append + Load e concorrência otimista

```csharp
public class PostgresEventStoreTests(PostgresFixture fx) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task Append_DepoisLoad_RetornaEventosNaOrdem()
    {
        var store = new PostgresEventStore(/* datasource de fx */);
        var id = Guid.NewGuid();

        await store.AppendAsync(id, new IDomainEvent[] { new AccountOpened(id, "BRL") }, -1, default);
        var history = await store.LoadAsync(id, default);

        history.Should().ContainSingle().Which.Should().BeOfType<AccountOpened>();
    }

    [Fact]
    public async Task Append_DuasEscritasNaMesmaVersao_UmaFalhaPorConcorrencia()
    {
        var store = new PostgresEventStore(/* ... */);
        var id = Guid.NewGuid();
        await store.AppendAsync(id, new IDomainEvent[] { new AccountOpened(id, "BRL") }, -1, default);

        // ambas assumem versão 0 (helper Dep monta um TransactionPosted balanceado)
        var a = store.AppendAsync(id, new IDomainEvent[] { Dep(id, 100) }, 0, default);
        var b = store.AppendAsync(id, new IDomainEvent[] { Dep(id, 200) }, 0, default);

        var act = async () => await Task.WhenAll(a, b);
        await act.Should().ThrowAsync<ConcurrencyException>(); // uma das duas perde
    }
}
```

## Helper — monta um depósito balanceado (partida dobrada, ADR-0008)

```csharp
// Um depósito = credita a conta + debita a contrapartida do sistema (soma zero, RN-1).
static TransactionPosted Dep(Guid accountId, long cents) =>
    new(Guid.NewGuid(), new[]
    {
        new LedgerEntry(accountId, EntryDirection.Credit, cents),
        new LedgerEntry(SystemAccounts.ExternalFunding, EntryDirection.Debit, cents),
    }, "deposit");
```

## Projeção populando read model — poll de condição (SEM Sleep)

```csharp
[Fact]
public async Task AoConsumirTransactionPosted_ReadModelRefleteSaldo()
{
    // ... publica Dep(id, 5_000) no Kafka de teste; projeção consome e escreve no Mongo ...

    var view = await Eventually(() => readModel.GetByIdAsync(id, default),
        cond: v => v is { BalanceCents: 5_000 }, timeout: TimeSpan.FromSeconds(5));

    view!.BalanceCents.Should().Be(5_000);
}

// Helper: repete até a condição ou estoura o timeout. Substitui Thread.Sleep.
static async Task<T?> Eventually<T>(Func<Task<T?>> get, Func<T?, bool> cond, TimeSpan timeout)
{
    var deadline = DateTime.UtcNow + timeout;
    while (DateTime.UtcNow < deadline)
    {
        var v = await get();
        if (cond(v)) return v;
        await Task.Delay(100);
    }
    throw new TimeoutException("Condição não satisfeita dentro do timeout.");
}
```

## Erros que a IA comete aqui (evite)

- `await Task.Delay(3000)` "para dar tempo da projeção rodar" — use `Eventually`.
- Compartilhar estado entre testes: limpe schema/coleção por teste, ou container por classe.
- Mockar `IEventStore` num teste que deveria provar o adapter Postgres real.
