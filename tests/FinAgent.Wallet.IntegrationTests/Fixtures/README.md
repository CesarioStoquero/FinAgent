# Convenção: testes `[requer contêiner]` sem Docker

Todo teste de integração que precisa de um Postgres/Mongo/Kafka real via Testcontainers
começa com:

```csharp
[Fact]
public async Task Meu_Teste()
{
    Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
    var ct = TestContext.Current.CancellationToken;
    // ...
}
```

- `DockerAvailability.IsAvailable` (`Fixtures/DockerAvailability.cs`) tenta rodar
  `docker info` uma única vez por execução e cacheia o resultado.
- `Assert.SkipUnless` é um recurso do xUnit v3 (`xunit.v3.assert`): quando a condição é
  falsa, o teste aparece como **IGNORADO** no relatório — não como falha, e não
  desaparece silenciosamente. Isso é o que mantém `dotnet test` verde numa máquina sem
  Docker, sem esconder a existência do teste.
- As fixtures (`PostgresFixture`, `MongoFixture`, `KafkaFixture`) só constroem e sobem o
  container dentro de `InitializeAsync`, e só se `DockerAvailability.IsAvailable` for
  verdadeiro — construir o `*Builder().Build()` fora dessa guarda lança
  `DockerUnavailableException` na hora, antes de qualquer teste rodar.
- Quando Docker estiver disponível (CI, ou a máquina do desenvolvedor com Docker Desktop
  rodando), os mesmos testes sobem os containers de verdade e executam a asserção —
  nenhum código de produção nem de teste muda.
