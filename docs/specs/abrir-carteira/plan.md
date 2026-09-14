# Plano técnico: Abrir uma carteira

- **Spec:** `docs/specs/abrir-carteira/spec.md` (SPEC-2026-09-02-abrir-carteira, Validada)
- **Backlog:** BL-5 · **Módulo:** wallet · **Autor:** software-engineer · **Data:** 2026-09-02
- **Tarefas:** `docs/specs/abrir-carteira/tasks.md` (24 tarefas, todas `[backend]`)

> Este é o plano do COMO. O QUE e o PORQUÊ estão na spec. Nenhuma capacidade fora da spec
> é introduzida aqui: **não** há depósito, saque, estorno, extrato nem consulta de saldo.

> **Nota (2026-09-02, ADR-0011):** este plano descreve a DDL do event store como um script
> `.sql` embutido (`001_event_store.sql`) aplicado de forma idempotente na subida do
> processo (`EventStoreSchema`). Isso foi substituído por **migração EF Core versionada**
> (`src/FinAgent.Wallet.Infrastructure/Persistence/Migrations/`) — ver ADR-0011 para o
> princípio, a ferramenta e a fronteira. O histórico abaixo não foi reescrito.

---

## 1. Resumo do desenho

A carteira do titular é uma **`Account` do tipo `Customer`** no ledger de partida dobrada
(ADR-0008). Abrir a carteira é **um único evento `AccountOpened`** anexado como versão 0 do
stream do agregado no event store Postgres (ADR-0007), publicado no Kafka (ADR-0004) e
projetado num documento de conta no Mongo (ADR-0005) com saldo 0.

A abertura **não move dinheiro** — logo NÃO existe `TransactionPosted` aqui e a invariante de
soma zero (RN-1) não é exercitada nesta feature (ela chega com BL-1). A regra de partida
dobrada aparece só como *modelo*: os tipos `LedgerEntry`, `EntryDirection`, `AccountType` e a
conta de contrapartida `SystemAccounts.ExternalFunding` nascem aqui porque `AccountOpened`
e a projeção já dependem do vocabulário, mas nenhum lançamento é postado.

**A unicidade "uma carteira por titular" (RF-4 / RN-15) é estrutural, não uma consulta:**
o `walletId` é **derivado deterministicamente do `ownerId`** (ver D2). O segundo pedido de
abertura cai no mesmo stream, o handler encontra histórico não-vazio e devolve a carteira
existente sem novo append. Nem lookup, nem tabela de índice, nem risco de corrida.

Esta é a **primeira feature do projeto**: ela carrega as fundações mínimas do Domain
(`Money` em centavos, `AggregateRoot`, `IDomainEvent`, portas) e o esqueleto de cada adapter.

---

## 2. Mapa de impacto

### 2.1 O que já existe (verificado com Grep/Glob em `src/` e `tests/`)

| Artefato | Estado hoje |
|----------|-------------|
| `FinAgent.slnx` + 6 projetos `src/` + 2 de teste | Criados no BL-4, **vazios de domínio** |
| `Directory.Build.props` | Fixa `net10.0`, `nullable`, `TreatWarningsAsErrors` |
| `global.json` | Runner `Microsoft.Testing.Platform` (xUnit v3) |
| `docker-compose.yml` | Já sobe `postgres:16`, `mongo:7`, `kafka:3.7` — **reaproveitar, não recriar** |
| `src/FinAgent.Wallet.Api/Program.cs` | Só um `MapGet("/")` de bootstrap |
| `tests/**/SmokeTests.cs` | Só provam o harness |
| Pacotes de teste | AwesomeAssertions 9.6.0, xunit.v3 4.0.0, Testcontainers.{PostgreSql,MongoDb,Kafka} 4.14.0 — **já referenciados** |
| Código de domínio (`Account`, `Money`, `IEventStore`, `AccountOpened`) | **Não existe nada.** Grep retornou vazio. |

**Conclusão: greenfield.** Não há evento existente para versionar, não há replay para quebrar,
não há read model para migrar. O risco de regressão nesta feature é essencialmente zero — o
risco real é de *desenho*, porque tudo o que nascer aqui vira contrato para BL-1, BL-2, BL-3,
BL-6 e BL-7.

### 2.2 Criado do zero

| Camada | Artefatos criados |
|--------|-------------------|
| Domain | `DomainException`, `ConcurrencyException`, `WalletNotFoundException`, `Money`, `Currencies`, `IDomainEvent`, `AggregateRoot`, `EntryDirection`, `LedgerEntry`, `AccountType`, `SystemAccounts`, `AccountOpened`, `Account`, `WalletIdentity`, `EventEnvelope`, portas `IEventStore`, `IEventPublisher`, `IWalletReadModel` + DTO `WalletView` |
| Application | `ICommand`/`ICommandHandler`/`Unit`, `OpenWalletCommand`, `OpenWalletResult`, `OpenWalletHandler`, `DependencyInjection` |
| Infrastructure | `EventTypeRegistry`, `JsonEventSerializer`, DDL `001_event_store.sql` + `EventStoreSchema`, `PostgresEventStore`, `KafkaEventPublisher`, `AccountDocument`, `LedgerProjection`, `WalletProjectionWorker`, `MongoWalletReadModel`, adapters in-memory de dev, `DependencyInjection` |
| Api | `POST /wallets`, `OwnerContext` (borda de RN-14), mapeamento de erro para ProblemDetails, composição em `Program.cs` |
| IA (MCP) | tool `abrir_carteira` no `FinAgent.Ai.McpServer` |
| Testes | fakes `InMemoryEventStore`/`RecordingEventPublisher`, testes de unidade do domínio e do handler, teste de arquitetura, testes de integração com Testcontainers, teste de API com `WebApplicationFactory` |

### 2.3 Alterado (e o risco de cada alteração)

| Arquivo | Mudança | Risco |
|---------|---------|-------|
| `src/FinAgent.Wallet.Api/Program.cs` | Troca o `MapGet("/")` pela composição de DI + `MapOpenApi()` + endpoint de carteira | Baixo. Nada depende do "Hello World". |
| `src/FinAgent.Wallet.Infrastructure/*.csproj` | Adiciona `Npgsql`, `MongoDB.Driver`, `Confluent.Kafka`, `Microsoft.Extensions.*` | Baixo, **desde que só aqui**. O teste de arquitetura (T13) trava isso. |
| `tests/FinAgent.Wallet.UnitTests/*.csproj` | Nenhuma. Continua referenciando só Domain + Application. | — |
| `tests/FinAgent.Wallet.IntegrationTests/*.csproj` | Adiciona `Microsoft.AspNetCore.Mvc.Testing` para o teste de endpoint | Baixo |
| `docker-compose.yml` | Opcional: descomentar o serviço `wallet-api` | Baixo |
| `docs/backlog.md` | BL-5 muda de "spec escrita" para "entregue" ao fim | Nenhum |

**Contratos que esta feature CONGELA para as próximas** (mudá-los depois quebra replay ou
projeção): o payload de `AccountOpened`, o nome de contrato do evento no registry, o schema
da tabela de eventos, o nome do tópico Kafka e o shape do `AccountDocument`. Trate cada um
como versionado desde o dia 1 — ver riscos R1 e R2.

---

## 3. Decisões técnicas

### D1 — O agregado é `Account`, o vocabulário externo é "carteira"

ADR-0008 é lei: o agregado se chama `Account` e tem `AccountType.Customer`. A palavra
"carteira" fica na borda (rota `/wallets`, `WalletView`, tool MCP `abrir_carteira`) e nos
tipos de leitura, exatamente como os templates canônicos das skills já fazem
(`IWalletReadModel` + `WalletView` no `reference/query-handler.md`, `AccountDocument` no
`reference/projection.md`). Não renomeie nenhum dos dois lados.

### D2 — `walletId` derivado deterministicamente do `ownerId` (resolve RF-4 / RN-15)

`WalletIdentity.ForOwner(ownerId)` produz um GUID estável: SHA-256 de
(namespace fixo do FinAgent + bytes do `ownerId`), 16 primeiros bytes, com os bits de
versão/variante RFC 9562 ajustados. É função pura, mora no Domain e não faz I/O.

- **Por quê:** unicidade por titular é uma invariante *entre* agregados, que o Event Sourcing
  não resolve dentro do agregado. A alternativa clássica é uma tabela de índice
  `owner_id -> account_id` com constraint única — mais peças, mais um write a coordenar e
  ainda assim sujeito a corrida entre o insert do índice e o append do evento.
- **Como fecha a corrida:** duas aberturas simultâneas do mesmo titular geram o mesmo
  `walletId` e ambas tentam `AppendAsync(..., expectedVersion: -1)`. A PK `(aggregate_id,
  version)` do event store deixa exatamente uma passar; a perdedora recebe
  `ConcurrencyException`, recarrega e devolve a carteira existente. RF-11 (nada parcialmente
  criado) sai de graça: ou o append acontece inteiro, ou não acontece.
- **Consequência assumida:** enquanto a derivação for essa, é *impossível* um titular ter
  duas carteiras — que é exatamente a v1 (spec seção 3). Se um dia isso mudar, a migração é
  trocar `WalletIdentity` por um id aleatório + tabela de índice; as carteiras já abertas
  mantêm o id, porque o id é gravado no evento e não recalculado no replay.

### D3 — A escrita NÃO devolve saldo; devolve identidade

`OpenWalletResult(WalletId, OwnerId, Currency, AlreadyExisted)`. A skill de Event Sourcing
proíbe explicitamente "retornar o saldo direto da escrita — quem lê saldo é a query". O
critério de aceite "saldo 0 centavos" é provado em dois lugares legítimos:
`Account.Open(...).Balance == Money.Zero("BRL")` (teste de unidade do agregado) e
`AccountDocument.BalanceCents == 0` (teste de integração da projeção). A consulta de saldo
pela API é BL-2 e não entra aqui.

### D4 — `EventEnvelope`: divergência deliberada e mínima do template

O template `reference/command-handler.md` define `IEventPublisher.PublishAsync(IReadOnlyList<IDomainEvent>)`,
mas o template `reference/projection.md` exige um `eventVersion` para a idempotência do
upsert. Os dois não fecham. Resolução: o Domain ganha

```csharp
public sealed record EventEnvelope(Guid AggregateId, int Version, DateTimeOffset OccurredAt, IDomainEvent Event);
```

`AppendAsync` passa a **retornar** os envelopes que gravou (com a versão atribuída e o
`occurred_at` do banco) e `PublishAsync` recebe envelopes. A ordem obrigatória
`load -> rehydrate -> regra -> append -> publish -> clear` continua intacta. Motivos: (a) o
timestamp não entra no evento de domínio, então o replay permanece determinístico; (b) a
projeção recebe a versão sem inferir nada; (c) o consumidor Kafka fica idempotente de fato.
**Follow-up (fora desta feature):** atualizar
`.claude/skills/dotnet-event-sourcing-cqrs/reference/command-handler.md` para refletir isso.

### D5 — Portas primeiro, adapters reais depois; fakes só nos testes

`IEventStore`, `IEventPublisher` e `IWalletReadModel` são portas **no Domain** (regra
hexagonal). As tarefas T1–T12 entregam Domain + Application completos e testados usando
**test doubles no projeto de teste** (`InMemoryEventStore`, `RecordingEventPublisher`) — sem
Docker, sem container, ciclo de TDD rápido. Os adapters reais (T14–T20) vêm depois, cada um
com seu teste de integração Testcontainers, **marcados `[requer contêiner]`**.
Adicionalmente, a Infrastructure ganha adapters in-memory de *desenvolvimento*
(`Wallet:Store=InMemory`) para que a API rode e o endpoint seja testável de ponta a ponta
antes de qualquer container existir — é conveniência de execução local, nunca o default
de produção.

### D6 — Event store: schema e serialização versionáveis

Tabela única `wallet_events`:

| coluna | tipo | papel |
|--------|------|-------|
| `global_seq` | `bigserial` | ordem global (base de replay/catch-up futuro) |
| `aggregate_id` | `uuid` | stream |
| `version` | `int` | posição no stream |
| `event_type` | `text` | **nome de contrato**, ex. `wallet.account-opened` |
| `payload` | `jsonb` | dados do evento |
| `occurred_at` | `timestamptz` | carimbo do append |
| PK | `(aggregate_id, version)` | concorrência otimista (ADR-0007) |

`event_type` é um nome de contrato mapeado por um `EventTypeRegistry` explícito — **nunca**
o nome CLR do tipo, e **nunca** `TypeNameHandling`. Renomear a classe C# não pode quebrar o
replay (RN-3). Append é uma transação única com todos os eventos; violação de PK vira
`ConcurrencyException`. RN-3 é reforçada na infraestrutura: nenhum `UPDATE`/`DELETE` na
tabela em todo o código, e a role da aplicação recebe apenas `SELECT, INSERT` (a revogação
de `UPDATE`/`DELETE` faz parte da DDL e é o que torna "o passado é imutável" verificável).

### D7 — Kafka: tópico, chave e ordenação

Tópico `finagent.wallet.events`, **key = `aggregateId`** (garante ordem por carteira dentro
da partição, que é a única ordem de que a projeção precisa). Mensagem = o `EventEnvelope`
serializado (`aggregateId`, `version`, `occurredAt`, `eventType`, `payload`). Consumidor com
`enable.auto.commit=false`, commit após projetar (at-least-once + projeção idempotente).

### D8 — Projeção hospedada na própria Wallet Api (v1)

`WalletProjectionWorker` é um `BackgroundService` registrado no processo da Api. Motivo:
um deployable a menos na v1; extrair para um worker próprio é troca de host, não de
código, porque a projeção só depende de portas. Revisitar quando a carga justificar.

### D9 — Dual write (append -> publish) aceito, com mitigação nomeada

A skill fixa a ordem `append -> publish -> clear`. Se o publish falhar depois do append, o
evento existe no ledger (fonte da verdade intacta) mas o read model não o vê. Nesta feature
o impacto é limitado: a resposta da API vem do write side, então o titular recebe o
`walletId` correto de qualquer forma; só a leitura futura (BL-2/BL-3) ficaria atrasada.
**Não** introduzimos outbox agora (seria capacidade fora da spec) — fica registrado como
risco R3 e candidato a item de backlog próprio.

### D10 — Identidade do titular na borda (RN-14)

O `ownerId` **nunca** vem do corpo do request (aceitá-lo no body permitiria abrir carteira em
nome de terceiro, violando RN-14). A Api resolve o titular do contexto do chamador através de
um único ponto, `OwnerContext`. Na v1, sem ADR de autenticação, esse ponto lê o header
`X-Owner-Id`; trocar por JWT/OIDC depois é mudar essa classe e nada mais. O mesmo vale para a
tool MCP: o `ownerId` vem do contexto da conversa, não de um argumento que o LLM preencha.
Ver pendência P1.

### D11 — `Guid` puro nos ids, sem strongly-typed ids

Os templates canônicos (`LedgerEntry(Guid AccountId, ...)`, portas com `Guid aggregateId`)
usam `Guid`. Introduzir `WalletId`/`OwnerId` como record structs melhoraria a tipagem mas
divergiria de todo o material de referência logo na primeira feature. Decisão: `Guid`, com o
nome do parâmetro carregando o significado. Reavaliar se aparecer confusão real de argumentos.

### D12 — `Money` com `string Currency` + constante `Currencies.Brl`

Idêntico ao template. Um value object `Currency` seria mais rico, mas `Money` é o tipo mais
copiado do projeto e divergir dele custa mais do que ganha. `Currencies.Brl = "BRL"` evita
literal solto. RN-9 vive em `Money.EnsureSame` (moedas diferentes lançam) e, quando houver
operação (BL-1/BL-6), também numa guarda do agregado contra `_currency`.

### D13 — Sem PII, e a fronteira fica explícita

`AccountOpened` carrega **apenas** `accountId`, `ownerId` (identificador opaco), `currency` e
`type`. Nenhum nome, CPF, e-mail ou telefone entra em evento, log, payload Kafka ou documento
Mongo (RF-10). Isso satisfaz LGPD-1 e LGPD-9 por construção, mesmo elas sendo ⚠ hoje. Não há
"onde o PII mora" porque não há PII: se algum dia a carteira precisar de dado pessoal, ele
**não** entra no evento — entra num store de titular separado, referenciado por `ownerId`, e
as regras LGPD-1..9 precisam virar ✓ antes (spec seção 9).

---

## 4. Desenho por camada

### 4.1 Domain (`FinAgent.Wallet.Domain`) — zero `ProjectReference`

```
Errors/           DomainException, ConcurrencyException, WalletNotFoundException
Money.cs          readonly record struct Money(long Cents, string Currency) + Currencies.Brl
Events/           IDomainEvent, AccountOpened, EventEnvelope
Ledger/           EntryDirection, LedgerEntry, AccountType, SystemAccounts
AggregateRoot.cs  Raise / Apply / Replay / Version / UncommittedEvents / ClearUncommittedEvents
Account.cs        Open, Rehydrate, Apply
WalletIdentity.cs ForOwner(Guid ownerId) -> Guid determinístico
Ports/            IEventStore, IEventPublisher, IWalletReadModel + WalletView
```

`Account.Open(ownerId, currency)` calcula o id via `WalletIdentity.ForOwner`, valida a moeda
e faz `Raise(new AccountOpened(id, ownerId, currency, AccountType.Customer))`. `Apply` só muda
estado (`Id`, `_ownerId`, `_currency`, `Balance = Money.Zero(currency)`) e **nunca valida** —
`Apply` reexecuta no replay e não pode falhar. Toda validação acontece antes do `Raise`.

### 4.2 Application (`FinAgent.Wallet.Application`) — ref: Domain

```
Abstractions/     ICommand<T>, ICommandHandler<TCmd,TRes>, Unit
OpenWallet/       OpenWalletCommand(Guid OwnerId)
                  OpenWalletResult(Guid WalletId, Guid OwnerId, string Currency, bool AlreadyExisted)
                  OpenWalletHandler
DependencyInjection.cs
```

`OpenWalletHandler`, na ordem canônica:

```
1. walletId = WalletIdentity.ForOwner(cmd.OwnerId)
2. history  = await store.LoadAsync(walletId, ct)
3. history.Count > 0  ->  Rehydrate e retorna AlreadyExisted: true   (RF-4/RN-15, sem append)
4. account  = Account.Open(cmd.OwnerId, Currencies.Brl)              (regra no agregado)
5. envelopes = await store.AppendAsync(walletId, account.UncommittedEvents, expectedVersion: -1, ct)
6. await publisher.PublishAsync(envelopes, ct)
7. account.ClearUncommittedEvents()
8. catch ConcurrencyException -> recarrega e retorna AlreadyExisted: true (corrida, D2)
```

Nenhuma regra de negócio no handler: ele orquestra. A moeda BRL é fixada aqui como *política
de aplicação* (o pedido não a informa, RF-3) e a *invariante* de moeda única vive no agregado.

### 4.3 Infrastructure (`FinAgent.Wallet.Infrastructure`) — ref: Application, Domain

```
Serialization/  EventTypeRegistry (nome de contrato <-> tipo), JsonEventSerializer
Persistence/    001_event_store.sql, EventStoreSchema (aplicador de DDL),
                PostgresEventStore : IEventStore
Messaging/      KafkaEventPublisher : IEventPublisher, WalletProjectionWorker (BackgroundService)
Projections/    AccountDocument, LedgerProjection
ReadModel/      MongoWalletReadModel : IWalletReadModel
Dev/            InMemoryEventStore, InMemoryEventPublisher, InMemoryWalletReadModel (Wallet:Store=InMemory)
DependencyInjection.cs
```

Todos os adapters são `internal`, expostos só por DI. É o **único** projeto que enxerga
`Npgsql`, `MongoDB.Driver` e `Confluent.Kafka`.

`LedgerProjection.ProjectAsync(AccountOpened)` faz upsert do `AccountDocument`
(`Id`, `OwnerId`, `Currency`, `BalanceCents = 0`, `Version`), guardado por
`Version < eventVersion` — reprocessar a mesma mensagem não altera nada.

### 4.4 Api (`FinAgent.Wallet.Api`) — ref: Application, Domain

`POST /wallets` (sem corpo; o titular vem do `OwnerContext`, D10):

| Situação | Status | Corpo |
|----------|--------|-------|
| Carteira criada agora | `201 Created` + `Location: /wallets/{walletId}` | `{ walletId, ownerId, currency: "BRL", alreadyExisted: false }` |
| Titular já tinha carteira (RF-4) | `200 OK` | `{ walletId, ownerId, currency: "BRL", alreadyExisted: true }` |
| Titular não identificado (RN-14) | `401 Unauthorized` | ProblemDetails |

Mapeamento de erro para ProblemDetails, centralizado num handler de exceção:

| Exceção | Status | `title` / motivo |
|---------|--------|------------------|
| `WalletNotFoundException` | `404` | `carteira inexistente` (RN-12) |
| `DomainException` de moeda | `422` | `moeda divergente` (RN-9) |
| `DomainException` genérica | `422` | mensagem da regra |
| `ConcurrencyException` que escapou | `409` | conflito de concorrência |

OpenAPI pelo `AddOpenApi()`/`MapOpenApi()` nativos. **Swashbuckle é proibido** (ADR-0009).

**Fronteira de escopo — RN-12:** esta feature *cria a pré-condição* e entrega o vocabulário
da recusa (`WalletNotFoundException` + mapeamento para "carteira inexistente"), testados por
unidade. A recusa ponta a ponta em depósito, saque, estorno, saldo e extrato pertence às
specs dessas operações; replicar aqui endpoints que não existem seria inventar capacidade.

### 4.5 Camada de IA (`FinAgent.Ai.McpServer`) — ADR-0006

Tool `abrir_carteira`: sem argumento de titular (o `ownerId` vem do contexto da conversa,
RN-14/D10), sem confirmação explícita (RN-13 reserva confirmação para o que move dinheiro), e
chamando o mesmo `POST /wallets` — a tool não fala com o Domain nem com o banco. Pedido
repetido devolve a carteira existente, porque a idempotência já está no handler.

---

## 5. Conformidade regra a regra

| Regra | Onde é cumprida | O que prova |
|-------|-----------------|-------------|
| RF-1 / identificador estável | `WalletIdentity.ForOwner` (Domain) | T5 (determinismo + valor-âncora) |
| RF-2 / saldo 0, sem movimento | `Account.Apply(AccountOpened)`; `AccountDocument.BalanceCents=0` | T4, T18 |
| RF-3 / RN-9 BRL fixa e imutável | `Currencies.Brl` no handler; `_currency` só escrito em `AccountOpened`; `Money.EnsureSame` | T1, T4, T11 |
| RF-4 / RN-15 uma por titular | D2 + caminho idempotente do handler | T12 (repetição e corrida) |
| RF-5 / RN-12 carteira inexistente | `WalletNotFoundException` + mapeamento ProblemDetails | T6, T23 |
| RF-6 / RN-9 moeda divergente | `Money.EnsureSame` lança `DomainException` | T1 |
| RF-7 / RN-3 abertura definitiva | Append-only + PK `(aggregate_id, version)` + role sem `UPDATE`/`DELETE` | T15, T16 |
| RF-8 / RN-14 só o próprio titular | `OwnerContext` na Api; tool MCP sem argumento de titular | T22, T24 |
| RF-9 / apta a operar | Stream existe na versão 0; `Rehydrate` devolve conta utilizável | T4, T12 |
| RF-10 / sem PII (LGPD-1, LGPD-9) | `AccountOpened` só com ids opacos; nada de PII em log/Kafka/Mongo | T4 (formato do evento) + revisão |
| RF-11 / nada parcialmente criado | Append transacional único; falha não deixa stream | T15 |
| RN-1 soma zero | **Não exercitada:** a abertura não movimenta. Tipos criados, nenhum lançamento postado | — |
| RN-2 saldo derivado | Não há coluna de saldo no write side; o Mongo é projeção descartável | T13 (arquitetura), T18 |
| RN-8 centavos `long` | `Money(long Cents, ...)`; zero `decimal`/`double` no projeto | T1, T13 |
| RN-13 agente sem confirmação | Tool `abrir_carteira` não pede confirmação (não move dinheiro) | T24 |

---

## 6. Riscos

| # | Risco | Gravidade | Mitigação |
|---|-------|-----------|-----------|
| R1 | O payload de `AccountOpened` vira contrato eterno; mudá-lo depois quebra o replay (RN-3) | Alta | `EventTypeRegistry` com nome de contrato desde o dia 1; campo novo só como opcional; nunca remover/renomear campo. Incluir `AccountType` **agora**, não depois. |
| R2 | Mudar o schema do `AccountDocument` quebra a projeção já materializada | Média | Read model é **descartável**: a correção é reprojetar do event store. Isso exige um catch-up reader — não construído aqui, mas o `global_seq` da tabela já o viabiliza (D6). |
| R3 | Dual write append->publish: publish falha e o read model nunca vê a carteira | Média | Aceito na v1 (D9). A resposta da API não depende do read model. Candidato a item de backlog "outbox transacional". |
| R4 | `WalletIdentity` determinístico impede duas carteiras por titular para sempre | Baixa | É o comportamento desejado da v1 (spec seção 3). Caminho de saída documentado em D2. |
| R5 | Testcontainers exige Docker; sem daemon, T14–T20 não rodam | Média | Marcadas `[requer contêiner]` e separadas por trait, para a suíte de unidade rodar sozinha. Adapters in-memory (T20) mantêm a API executável sem Docker. |
| R6 | SDK MCP em preview (ADR-0006), versão não fixada em nenhum ADR | Baixa | T24 é a última tarefa e isolável; pendência P2. |
| R7 | Primeira feature = mais decisão de fundação do que de negócio; risco de sobre-engenharia | Média | Nada além do que a spec exige: sem snapshots, sem plano de contas, sem multi-moeda, sem outbox, sem estados pendentes (RN-7 não adotada). |

---

## 7. Fora de escopo (explicitamente NÃO construir aqui)

Depósito, saque, estorno, consulta de saldo, consulta de extrato, `TransactionPosted`,
encerramento/bloqueio de carteira, segunda carteira por titular, multi-moeda ou conversão,
snapshots do agregado, outbox transacional, catch-up/reprojeção, autenticação real,
tarifa de abertura, qualquer campo de dado pessoal, qualquer tela (não há frontend nesta
feature).

---

## 8. Pendências

Nenhuma pendência **bloqueia** o início da implementação. Duas decisões seguem abertas e
devem ser fechadas antes de produção:

- **[NEEDS CLARIFICATION] P1 — Como o titular é autenticado?** A spec exige RN-14 ("só o
  titular opera a própria carteira") mas nenhum item do backlog cobre autenticação e nenhum
  ADR a decide. O plano isola isso em `OwnerContext` (D10), que na v1 lê o header
  `X-Owner-Id` — suficiente para desenvolvimento e teste, **inseguro para produção**.
  Perguntas a decidir: o `ownerId` é emitido pelo FinAgent ou vem de um IdP externo? Isso
  vira um item de backlog próprio (e provavelmente um ADR)?
- **[NEEDS CLARIFICATION] P2 — Versão do SDK MCP.** ADR-0009 manda fixar versões, mas nenhum
  ADR fixa a do SDK MCP em C#, que está em preview (ADR-0006). Confirmar a versão antes da
  T24; se não houver decisão, T24 fica bloqueada sem afetar T1–T23.
