# ADR-0011: Schema e dados só mudam por migração (EF Core Migrations + Fluent API)

- **Status:** Aceito
- **Data:** 2026-09-02

## Contexto
A primeira entrega (BL-5, abrir carteira) criou a tabela `wallet_events` aplicando um
arquivo `.sql` embutido a cada subida do processo, com `CREATE TABLE IF NOT EXISTS` como
mecanismo de "idempotência". Isso funciona enquanto existe exatamente UM script e ele nunca
muda — e falha silenciosamente em tudo o que vem depois:

- **Sem histórico:** não há como saber qual versão do schema um ambiente está rodando.
- **Sem ordem:** com dois ou mais scripts, nada garante que rodem na ordem certa, uma vez só.
- **Sem detecção de divergência:** `IF NOT EXISTS` faz o segundo `CREATE TABLE` virar no-op.
  Se o script mudar (coluna nova, índice novo), o ambiente que já tem a tabela **ignora a
  mudança** e diverge do código, sem erro.
- **Sem replay confiável:** num event store, o schema é a fundação do passado. Divergência de
  schema entre ambientes é divergência de leitura do ledger (ADR-0001, ADR-0007).

## Decisão

### 1. O princípio
**Toda criação ou alteração de objeto de banco E toda inserção de dados acontece por migração
versionada.** Nenhum `CREATE`/`ALTER`/`INSERT` roda fora de uma migração, e nenhuma migração
já aplicada é editada — mudança é sempre uma migração NOVA.

Vale explicitamente também para **dado**: seed, dado de referência e carga inicial entram por
migração, nunca por código de startup nem por comando manual no banco. Se não está numa
migração, ninguém sabe quais ambientes têm aquele dado.

### 2. A ferramenta: EF Core Migrations, modelo configurado por Fluent API
- **`Microsoft.EntityFrameworkCore` 10.0.11** + provider **`Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3** (alinhados com ADR-0009).
- O modelo é configurado **por Fluent API** (`IEntityTypeConfiguration<T>`), **nunca por Data
  Annotations** — a configuração fica na Infrastructure, junto do adapter, e não polui os
  tipos com atributos de persistência.
- As migrações vivem em `src/FinAgent.Wallet.Infrastructure/Persistence/Migrations/`.
- O EF mantém `__EFMigrationsHistory`: ordem e execução única garantidas por ele.

**Por que EF Core e não FluentMigrator/DbUp:** é o mecanismo de migração mantido pela
Microsoft, versionado junto com o .NET, e o padrão dominante em sistemas .NET grandes —
ferramental (`dotnet ef`), documentação, tooling de CI e conhecimento de mercado são
incomparavelmente maiores. FluentMigrator e DbUp resolvem o problema, mas são nicho: escolher
o nicho aqui custaria familiaridade sem ganho correspondente.

### 3. A fronteira: EF define o schema, Npgsql lê e escreve o ledger
**O EF Core entra como mecanismo de SCHEMA, não como caminho de dados do event store.** O
`PostgresEventStore` continua usando **Npgsql cru** para `LoadAsync`/`AppendAsync`.

O motivo é de domínio, não de gosto: o event store é um fluxo append-only com payload JSONB e
concorrência otimista pela PK `(aggregate_id, version)`. Change tracking, identity map e
`SaveChanges` do EF são maquinaria para estado mutável — exatamente o que a ADR-0001 diz que
o ledger não é. Colocar o EF no caminho de escrita do evento traria custo e risco sem
benefício.

Se um módulo futuro tiver read model relacional de verdade (estado mutável, consultas
ad hoc), usar EF ali é decisão daquele módulo — esta ADR não proíbe.

### 4. Quem aplica, e quando
- **Caminho canônico:** `dotnet ef database update` (ou o bundle de migração em deploy).
  Existe um `IDesignTimeDbContextFactory` para o `dotnet ef` funcionar sem subir a aplicação.
- **Conveniência de dev:** a Api pode aplicar na subida, mas só quando o store não for
  InMemory **e** a configuração `Wallet:Postgres:ApplyMigrationsOnStartup` estiver ligada —
  ligada por padrão apenas em Development. **Fora de Development, migração não roda sozinha.**

### 5. A exceção que continua condicional
`GRANT`, `REVOKE` e `CREATE ROLE` são objetos de **cluster**, não do banco, e o EF não os
modela. Eles entram na migração via `migrationBuilder.Sql(...)`, e o passo que cria a role
`wallet_app` segue condicional (`IF NOT EXISTS`), porque a role pode preexistir num banco
recém-criado. A condicionalidade fica restrita a esse caso — tabela e índice nunca.

Isso é importante: é a revogação de `UPDATE`/`DELETE` que sustenta **RN-3** (o passado é
fisicamente imutável). Ela passa a ser versionada junto da tabela que protege, em vez de ser
efeito colateral da subida do processo.

## Consequências
- (+) Schema e dado de cada ambiente ficam conhecidos e auditáveis (`__EFMigrationsHistory`).
- (+) Ordem e execução única garantidas; migração aplicada não roda de novo.
- (+) Ferramental de primeira classe: `dotnet ef migrations add`, `script`, `bundle`.
- (+) A garantia de RN-3 passa a ser versionada junto com a tabela que ela protege.
- (−) Entra um ORM na Infrastructure que o caminho de dados do ledger não usa — é preciso
  disciplina para o `DbContext` não virar, por inércia, o caminho de leitura do event store.
- (−) O `DbContext` precisa espelhar o schema real; se divergir, o diff do EF gera migração
  errada. Mitigação: teste de integração que aplica as migrações num Postgres limpo e confere
  o schema resultante.
- (−) Mudar schema ou dado exige disciplina: nunca editar migração aplicada.

## Alternativas consideradas
- **FluentMigrator:** migrações como classes C# sem exigir ORM — encaixaria melhor num serviço
  sem EF. Descartado por ser nicho perto do EF Core (ver Decisão 2).
- **DbUp:** SQL puro com diário `schemaversions`. Chegou a ser proposto nesta ADR e foi
  descartado pelo dono do projeto em 2026-09-02.
- **Manter DDL idempotente na subida:** é justamente o que esta ADR corrige.

## Relação com outros ADRs
- **ADR-0007** (Postgres como event store): esta ADR define COMO a tabela daquele ADR nasce.
- **ADR-0003** (hexagonal): a migração é detalhe de Infrastructure. `ArquiteturaTests` trava
  `Microsoft.EntityFrameworkCore` como assembly proibido no **Domain** — isso continua valendo
  e passa a ser a fronteira que impede o ORM de vazar para dentro.
- **ADR-0009** (stack e versões): EF Core 10.0.11 + Npgsql.EFCore.PostgreSQL 10.0.3 entram lá
  como versões fixadas.
