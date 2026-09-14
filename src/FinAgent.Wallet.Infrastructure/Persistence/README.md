# Schema do event store — migrações EF Core (ADR-0011)

O schema de `wallet_events` nasce e evolui por **migração EF Core**, nunca por DDL
aplicada na subida do processo. Leia o ADR-0011 inteiro antes de mexer aqui.

- `WalletEventStoreDbContext` + `Configurations/WalletEventRowConfiguration.cs`
  (Fluent API) só DEFINEM o schema. `PostgresEventStore` continua em Npgsql cru para
  `LoadAsync`/`AppendAsync` — o EF nunca entra no caminho de dados do ledger.
- `WalletEventStoreDbContextFactory` (`IDesignTimeDbContextFactory`) permite ao
  `dotnet ef` rodar sem subir a Api.

## Comandos

Da raiz do repositório (o tool manifest fica em `.config/dotnet-tools.json`;
`dotnet tool restore` uma vez se for a primeira vez neste clone):

```bash
# Nova migração, depois de mudar WalletEventRowConfiguration
dotnet ef migrations add NomeDaMigracao \
  --project src/FinAgent.Wallet.Infrastructure \
  --context WalletEventStoreDbContext \
  --output-dir Persistence/Migrations

# Caminho canônico de aplicação (dev/CI)
dotnet ef database update \
  --project src/FinAgent.Wallet.Infrastructure \
  --context WalletEventStoreDbContext

# Conferir que o modelo não tem mudança pendente sem migração gerada
dotnet ef migrations has-pending-model-changes \
  --project src/FinAgent.Wallet.Infrastructure \
  --context WalletEventStoreDbContext
```

A connection string do design-time vem de `FINAGENT_POSTGRES_CONNECTION`; sem a
variável, cai no default de dev que aponta para o Postgres do `docker-compose.yml` da
raiz (`Host=localhost;Port=5432;Database=finagent_events;Username=finagent;Password=finagent`).

## Quem aplica, e quando (ADR-0011 seção 4)

- **Caminho canônico:** `dotnet ef database update` (acima), ou o bundle de migração
  em deploy.
- **Conveniência de dev — Api aplica na subida:** só quando `Wallet:Store != InMemory`
  **e** `Wallet:Postgres:ApplyMigrationsOnStartup` estiver ligado. Ligado por padrão
  **só** em Development (`appsettings.Development.json`); fora de Development, migração
  não roda sozinha.
- **`--migrate` explícito:** `dotnet run --project src/FinAgent.Wallet.Api -- --migrate`
  aplica a migração pendente e encerra sem subir o Kestrel — independe de
  `ApplyMigrationsOnStartup`.

## A exceção que continua condicional

`GRANT`/`REVOKE`/`CREATE ROLE` são objetos de cluster que o EF não modela — entram na
migração inicial via `migrationBuilder.Sql(...)`. Só a criação da role `wallet_app`
segue condicional (`IF NOT EXISTS`, pode preexistir); tabela e índices nunca levam
`IF NOT EXISTS`. É essa revogação de `UPDATE`/`DELETE` que sustenta RN-3 (o passado é
fisicamente imutável).
