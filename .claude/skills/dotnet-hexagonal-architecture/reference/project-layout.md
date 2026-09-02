# Template canônico — Layout de solução e DI

Um serviço (ex.: Wallet) = 4 projetos. Copie os nomes e as referências exatas.

## Bootstrap da solution (greenfield — rode UMA vez)

Só quando não existe `.sln`/`.slnx` na raiz. As pastas de `src/` e `tests/` já existem com um
`_README.md` cada — crie o projeto DENTRO da pasta correspondente, não uma pasta nova.

```bash
dotnet new sln -n FinAgent

# Núcleo do serviço Wallet (a seta de referência só aponta para dentro)
dotnet new classlib -o src/FinAgent.Wallet.Domain
dotnet new classlib -o src/FinAgent.Wallet.Application
dotnet new classlib -o src/FinAgent.Wallet.Infrastructure
dotnet new web      -o src/FinAgent.Wallet.Api

# Camada agêntica (ADR-0006)
dotnet new console  -o src/FinAgent.Ai.McpServer
dotnet new console  -o src/FinAgent.Ai.Agent

dotnet sln add (ls -r src/*/*.csproj)      # PowerShell; no bash: src/*/*.csproj
```

Referências entre projetos — exatamente estas, nem uma a mais:

```bash
dotnet add src/FinAgent.Wallet.Application  reference src/FinAgent.Wallet.Domain
dotnet add src/FinAgent.Wallet.Infrastructure reference src/FinAgent.Wallet.Application src/FinAgent.Wallet.Domain
dotnet add src/FinAgent.Wallet.Api          reference src/FinAgent.Wallet.Application src/FinAgent.Wallet.Domain
```

`Domain` fica com ZERO `ProjectReference`. `Application` NUNCA referencia `Infrastructure`.

### Projetos de teste (ADR-0009: xUnit v3 + AwesomeAssertions)

**Confirme o template antes de usar:** rode `dotnet new list xunit` e veja se existe um
template de **xUnit v3** neste SDK. Se existir, use-o. Se NÃO existir, crie `classlib` e
adicione os pacotes à mão — nunca caia no template de xUnit v2 por conveniência:

```bash
dotnet new classlib -o tests/FinAgent.Wallet.UnitTests
dotnet add tests/FinAgent.Wallet.UnitTests package xunit.v3
dotnet add tests/FinAgent.Wallet.UnitTests package Microsoft.NET.Test.Sdk
dotnet add tests/FinAgent.Wallet.UnitTests package AwesomeAssertions
dotnet add tests/FinAgent.Wallet.UnitTests reference src/FinAgent.Wallet.Domain src/FinAgent.Wallet.Application

dotnet new classlib -o tests/FinAgent.Wallet.IntegrationTests
# + os mesmos pacotes, mais Testcontainers (.PostgreSql / .MongoDb / .Kafka)
dotnet add tests/FinAgent.Wallet.IntegrationTests reference src/FinAgent.Wallet.Infrastructure
```

**NUNCA** adicione `FluentAssertions` (licença comercial desde a v8) nem `Swashbuckle`
(a Api usa `AddOpenApi()`/`MapOpenApi()` nativos). Ver ADR-0009.

### Antes de dar o bootstrap por concluído

1. Nenhum `.csproj` redeclara `TargetFramework`, `Nullable`, `LangVersion` ou
   `TreatWarningsAsErrors` — tudo vem do `Directory.Build.props` da raiz (ADR-0009).
2. `dotnet build` limpo (warnings-as-errors ligado).
3. `dotnet test` roda e passa (mesmo com zero teste).
4. Os `_README.md` das pastas continuam onde estavam.

## Estrutura de projetos e referências

```
FinAgent.Wallet.Domain/           # 0 referências. Agregados, eventos, VOs, PORTAS.
FinAgent.Wallet.Application/       # ref: Domain. Commands/queries + handlers.
FinAgent.Wallet.Infrastructure/    # ref: Application, Domain. Adapters (PG/Mongo/Kafka).
FinAgent.Wallet.Api/               # ref: Application, Domain. Minimal API (entrada).
```

Regra visual: a seta de `<ProjectReference>` só pode apontar para a esquerda (para dentro).
`Domain` não tem nenhuma. `Infrastructure` NUNCA é referenciado por `Application`.

## Domain.csproj — provando o isolamento

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- SEM PropertyGroup de versão. `TargetFramework` (net10.0), `Nullable`,
       `LangVersion` e `TreatWarningsAsErrors` vêm do Directory.Build.props da RAIZ.
       NUNCA redeclare aqui: a versão mora em UM lugar só (ADR-0009). -->
  <!-- NENHUM ProjectReference. NENHUM pacote de infra. Se precisar de algo externo,
       vira uma porta (interface) aqui e um adapter na Infrastructure. -->
</Project>
```

## DependencyInjection.cs por camada

Cada camada expõe UM método de extensão. A Api compõe todos.

```csharp
// FinAgent.Wallet.Application/DependencyInjection.cs
namespace FinAgent.Wallet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddWalletApplication(this IServiceCollection s)
    {
        s.AddScoped<ICommandHandler<WithdrawCommand, Unit>, WithdrawHandler>();
        s.AddScoped<IQueryHandler<GetBalanceQuery, WalletView?>, GetBalanceHandler>();
        return s;
    }
}
```

```csharp
// FinAgent.Wallet.Infrastructure/DependencyInjection.cs
namespace FinAgent.Wallet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWalletInfrastructure(this IServiceCollection s, IConfiguration cfg)
    {
        s.AddScoped<IEventStore, PostgresEventStore>();       // adapter implementa porta do Domain
        s.AddScoped<IEventPublisher, KafkaEventPublisher>();
        s.AddScoped<IWalletReadModel, MongoWalletReadModel>();
        return s;
    }
}
```

```csharp
// FinAgent.Wallet.Api/Program.cs (composição)
builder.Services
    .AddWalletApplication()
    .AddWalletInfrastructure(builder.Configuration);
```

## Porta vs Adapter (o coração do padrão)

```csharp
// PORTA — Domain. O núcleo declara O QUE precisa, sem saber COMO.
namespace FinAgent.Wallet.Domain.Ports;
public interface IEventStore { /* LoadAsync / AppendAsync */ }
```

```csharp
// ADAPTER — Infrastructure. Aqui, e SÓ aqui, entra o Npgsql.
namespace FinAgent.Wallet.Infrastructure.Persistence;
internal sealed class PostgresEventStore(NpgsqlDataSource db) : IEventStore { /* ... */ }
```

## Erros que a IA comete aqui (evite)

- Criar `IEventStore` dentro de Infrastructure — porta é do Domain; senão o Domain
  passa a depender da Infra para enxergar a interface.
- Referenciar `Infrastructure` a partir de `Application` para "usar o repositório concreto".
- Marcar adapter como `public` sem necessidade — ele é `internal`, exposto só via DI.
