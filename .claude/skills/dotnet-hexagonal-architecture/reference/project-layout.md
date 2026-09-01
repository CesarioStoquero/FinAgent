# Template canônico — Layout de solução e DI

Um serviço (ex.: Wallet) = 4 projetos. Copie os nomes e as referências exatas.

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
