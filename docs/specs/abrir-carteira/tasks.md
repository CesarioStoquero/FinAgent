# Tarefas: Abrir uma carteira

- **Spec:** `docs/specs/abrir-carteira/spec.md` · **Plano:** `docs/specs/abrir-carteira/plan.md`
- **Backlog:** BL-5 · **Módulo:** wallet · **24 tarefas, todas `[backend]`** (a feature não tem UI)

## Como executar

- **Ordem é obrigatória.** Cada tarefa assume as anteriores prontas.
- **TDD (skill `dotnet-testing`):** escreva o teste, veja falhar, implemente o mínimo, refatore.
- **Nome do teste:** `Metodo_Cenario_ResultadoEsperado`.
- `dotnet build` limpo com `TreatWarningsAsErrors` é parte do "pronto" de toda tarefa.
- **`[requer contêiner]`** = precisa do Docker rodando (Testcontainers). Sem Docker, pule para
  a próxima tarefa sem container; T1–T13 e T20–T22 rodam sem Docker.
- Templates canônicos: `.claude/skills/dotnet-event-sourcing-cqrs/reference/`,
  `.claude/skills/dotnet-hexagonal-architecture/reference/project-layout.md`,
  `.claude/skills/dotnet-testing/reference/`.

### Onde cada teste mora

| Projeto de teste | Referencia | Recebe |
|------------------|------------|--------|
| `tests/FinAgent.Wallet.UnitTests` | Domain + Application | domínio, handlers, fakes, teste de arquitetura |
| `tests/FinAgent.Wallet.IntegrationTests` | Infrastructure (e a Api) | tudo que toca Infrastructure — **inclusive** o que não precisa de container (ex.: serialização), porque é o único projeto que a referencia |

---

## Fase 1 — Fundações do Domain (sem infra, sem Docker)

### T1 [backend] — Value object `Money` + `Currencies` + `DomainException` (Domain)

Cria `DomainException` e `readonly record struct Money(long Cents, string Currency)` com
`Zero`, `Add`, `Subtract` e `EnsureSame`. Copie de `reference/aggregate.md`, seção "Money".
Adiciona `public static class Currencies { public const string Brl = "BRL"; }`.

- **Comportamento:** dinheiro é inteiro de centavos (RN-8); somar/subtrair moedas diferentes
  lança `DomainException` "moeda divergente" (RN-9).
- **Teste (unidade, `MoneyTests`):** `Zero_EmBrl_TemZeroCentavosEMoedaBrl`;
  `Add_ComMoedaDivergente_LancaDomainException`; `Add_MesmaMoeda_SomaOsCentavos`.
- **Pronto quando:** `Money` não expõe nenhum `decimal`/`double`/`float` e os 3 testes passam.

### T2 [backend] — Base de agregado `AggregateRoot` + `IDomainEvent` (Domain)

Copie de `reference/aggregate.md`, seção "Base de agregado". Os nomes `Raise`, `Apply`,
`Replay`, `Version`, `UncommittedEvents`, `ClearUncommittedEvents` são **contrato**: não
renomeie.

- **Comportamento:** `Version` começa em `-1`; `Replay` incrementa por evento; `Raise` aplica
  e acumula; `ClearUncommittedEvents` esvazia sem tocar o estado.
- **Teste (unidade, `AggregateRootTests`, com um agregado-dublê minúsculo no próprio teste):**
  `Version_SemHistorico_EhMenosUm`; `Replay_ComDoisEventos_DeixaVersaoUm`;
  `ClearUncommittedEvents_DepoisDeRaise_EsvaziaSemMudarEstado`.
- **Pronto quando:** os 3 testes passam e a classe não conhece nenhum evento concreto.

### T3 [backend] — Vocabulário do ledger: `EntryDirection`, `LedgerEntry`, `AccountType`, `SystemAccounts` (Domain)

Copie de `reference/aggregate.md`. `AccountType` com `Customer` e `ExternalFunding` (ADR-0008).
**Nenhum lançamento é postado nesta feature** — estes tipos existem porque `AccountOpened` e a
projeção precisam do vocabulário.

- **Comportamento:** `SystemAccounts.ExternalFunding` é uma constante estável e conhecida.
- **Teste (unidade, `SystemAccountsTests`):**
  `ExternalFunding_EhUmGuidConstante_NaoMudaEntreExecucoes` (compara com o literal esperado —
  guarda contra alteração acidental que quebraria projeções futuras).
- **Pronto quando:** o teste passa e nada aqui tem comportamento (só dados).

### T4 [backend] — Evento `AccountOpened` + agregado `Account` (Domain)

`public sealed record AccountOpened(Guid AccountId, Guid OwnerId, string Currency, AccountType Type) : IDomainEvent;`
e `Account` com `Open(Guid ownerId, string currency)`, `Rehydrate(history)` e `Apply`.
`Apply` só muda estado — **nunca** valida (ele reexecuta no replay). Não crie `Deposit`,
`Withdraw`, `Reverse` nem `Post`: estão fora desta spec.

- **Comportamento:** `Open` levanta exatamente um `AccountOpened`; a conta nasce com
  `Balance == Money.Zero("BRL")` e nenhum movimento (RF-2); a moeda vem do evento e nunca é
  reescrita (RF-3); o evento carrega **só ids opacos**, nenhum dado pessoal (RF-10).
- **Teste (unidade, `AccountTests`):**
  `Open_ComTitularEMoedaBrl_LevantaUmAccountOpened`;
  `Open_RecemAberta_TemSaldoZeroEmBrl`;
  `Rehydrate_ComAccountOpened_ReconstroiSaldoZeroEVersaoZero`;
  `AccountOpened_NaoCarregaDadoPessoal` (assert sobre as propriedades do record: só
  `AccountId`, `OwnerId`, `Currency`, `Type`).
- **Pronto quando:** os 4 testes passam e `Account` não tem nenhum método de movimentação.

### T5 [backend] — `WalletIdentity.ForOwner` — id determinístico (Domain)

Função pura que deriva o `walletId` do `ownerId` (plano D2): SHA-256 de
(namespace fixo do FinAgent + bytes do `ownerId`), 16 bytes, bits de versão/variante RFC 9562.

- **Comportamento:** mesmo titular sempre gera o mesmo id (RF-1: único e **estável**);
  titulares diferentes geram ids diferentes; o valor não muda entre execuções nem entre
  máquinas.
- **Teste (unidade, `WalletIdentityTests`):**
  `ForOwner_ChamadaDuasVezes_DevolveOMesmoId`;
  `ForOwner_TitularesDiferentes_DevolvemIdsDiferentes`;
  `ForOwner_ComTitularConhecido_DevolveOValorAncora` (golden value fixo no teste — trava a
  derivação; se alguém mudar o algoritmo, carteiras existentes ficariam órfãs).
- **Pronto quando:** os 3 testes passam e a classe não faz I/O.

### T6 [backend] — Erros de domínio nomeados: `WalletNotFoundException`, `ConcurrencyException` (Domain)

`WalletNotFoundException : DomainException` com motivo `"carteira inexistente"` (RN-12);
`ConcurrencyException : DomainException` para o conflito de versão do event store (ADR-0007).

- **Comportamento:** o motivo da recusa é um texto estável, consumível pelo mapeamento de
  ProblemDetails (T23) e pelas specs de BL-1/BL-2/BL-3/BL-6/BL-7.
- **Teste (unidade, `DomainErrorsTests`):**
  `WalletNotFound_TemMotivoCarteiraInexistente`;
  `WalletNotFound_EhUmDomainException` (garante que um único `catch` na borda cobre as duas).
- **Pronto quando:** os 2 testes passam.

### T7 [backend] — Portas `IEventStore` e `IEventPublisher` + `EventEnvelope` (Domain)

Em `Domain/Ports/`, conforme `reference/command-handler.md` **com a divergência do plano D4**:
`AppendAsync` retorna `IReadOnlyList<EventEnvelope>` e `PublishAsync` recebe envelopes.
`EventEnvelope(Guid AggregateId, int Version, DateTimeOffset OccurredAt, IDomainEvent Event)`.

- **Comportamento:** contratos apenas — nenhuma implementação, nenhum tipo de infra.
- **Teste:** não tem teste próprio (é contrato); é validado por T9 (fakes), T12 (handler) e
  T13 (arquitetura).
- **Pronto quando:** compila e `FinAgent.Wallet.Domain.csproj` continua com **zero**
  `ProjectReference` e **zero** pacote de infra.

### T8 [backend] — Porta `IWalletReadModel` + DTO `WalletView` (Domain)

Copie de `reference/query-handler.md`. `WalletView(Guid WalletId, Guid OwnerId, string Currency, long BalanceCents)`.
**Não** crie query nem query handler: consulta de saldo é BL-2. A porta existe aqui porque a
projeção (T18/T19) precisa de um caminho de leitura para ser verificada.

- **Comportamento:** contrato de leitura, plano, feito para UI/agente — nunca o agregado.
- **Teste:** validado por T19 (adapter Mongo) e T13 (arquitetura).
- **Pronto quando:** compila e `WalletView` não expõe nenhum tipo de domínio.

---

## Fase 2 — Application (com fakes; ainda sem Docker)

### T9 [backend] — Contratos CQRS `ICommand`/`ICommandHandler`/`Unit` (Application)

Copie de `reference/command-handler.md`, seção "Contratos CQRS".

- **Comportamento:** contratos mínimos, sem framework de mediator.
- **Teste:** validado por T11.
- **Pronto quando:** compila e `FinAgent.Wallet.Application.csproj` referencia **só** o Domain.

### T10 [backend] — Fakes de teste `InMemoryEventStore` e `RecordingEventPublisher` (tests/UnitTests)

Test doubles em `tests/FinAgent.Wallet.UnitTests/Fakes/`, **não** em produção. O fake do event
store precisa ser fiel: guarda streams por `aggregateId`, atribui versões sequenciais e lança
`ConcurrencyException` quando `expectedVersion` não bate. Deve permitir simular a corrida
(um hook que injeta um append concorrente antes do seu).

- **Comportamento:** o fake se comporta como o Postgres nas regras que o handler depende.
- **Teste (unidade, `InMemoryEventStoreTests`):**
  `Append_EmStreamNovo_ComVersaoEsperadaMenosUm_Grava`;
  `Append_ComVersaoEsperadaErrada_LancaConcurrencyException`;
  `Load_DepoisDeAppend_DevolveEventosNaOrdem`.
- **Pronto quando:** os 3 testes passam (um fake não testado mente e esconde bug).

### T11 [backend] — Command handler `OpenWalletHandler` — caminho feliz (Application)

`OpenWalletCommand(Guid OwnerId)`, `OpenWalletResult(Guid WalletId, Guid OwnerId, string Currency, bool AlreadyExisted)`
e o handler na ordem canônica load -> rehydrate -> regra -> append -> publish -> clear.
Moeda fixada em `Currencies.Brl` (RF-3, o pedido não a informa). **Não** devolva saldo (D3).

- **Comportamento:** abrir para titular novo cria o stream na versão 0, publica o evento e
  devolve `AlreadyExisted: false` com o id derivado do titular.
- **Teste (unidade, `OpenWalletHandlerTests`, com os fakes de T10):**
  `Handle_TitularSemCarteira_GravaAccountOpenedNaVersaoZero`;
  `Handle_TitularSemCarteira_PublicaOEventoDepoisDoAppend` (verifica a ordem);
  `Handle_TitularSemCarteira_DevolveIdDerivadoDoTitularEMoedaBrl`;
  `Handle_DepoisDoPublish_LimpaUncommittedEvents`.
- **Pronto quando:** os 4 testes passam e o handler não tem nenhum `if` de regra de negócio
  (regra mora no agregado).

### T12 [backend] — `OpenWalletHandler` — idempotência e corrida (Application) — RF-4 / RN-15

Caminho em que o titular já tem carteira: `LoadAsync` volta não-vazio -> reidrata -> devolve
`AlreadyExisted: true` **sem append e sem publish**. Mais o `catch (ConcurrencyException)`
da corrida (D2): recarrega e devolve a existente.

- **Comportamento:** segundo pedido devolve a mesma carteira, não cria segunda, não altera
  saldo nem extrato (spec, Borda 1).
- **Teste (unidade, `OpenWalletHandlerTests`):**
  `Handle_TitularQueJaTemCarteira_DevolveOMesmoIdEAlreadyExistedTrue`;
  `Handle_TitularQueJaTemCarteira_NaoFazAppendNemPublish`;
  `Handle_TitularQueJaTemCarteira_NaoAlteraOSaldoDaCarteiraExistente` (arrange: histórico com
  `AccountOpened` + um `TransactionPosted` balanceado de 10.000 centavos, montado com o helper
  de `reference/integration.md`; assert: o saldo reidratado continua 10.000);
  `Handle_ComAberturaConcorrenteDoMesmoTitular_DevolveACarteiraExistente`.
- **Pronto quando:** os 4 testes passam.

### T13 [backend] — Teste de arquitetura hexagonal (tests/UnitTests)

Teste que lê os assemblies/`.csproj` e trava as invariantes do ADR-0003 e do ADR-0009.

- **Comportamento:** a regra de dependência é verificável por máquina, não por boa vontade.
- **Teste (unidade, `ArquiteturaTests`):**
  `Domain_NaoReferenciaNenhumOutroProjeto`;
  `Domain_NaoReferenciaAssemblyDeInfra` (Npgsql, MongoDB.Driver, Confluent.Kafka,
  Microsoft.EntityFrameworkCore);
  `Application_NaoReferenciaInfrastructure`;
  `Dominio_NaoUsaTipoDecimalOuDoubleEmValorMonetario` (varre as propriedades públicas dos
  tipos do Domain — RN-8).
- **Pronto quando:** os 4 testes passam. **A partir daqui, qualquer violação futura quebra a
  build.**

---

## Fase 3 — Infrastructure (adapters reais)

### T14 [backend] — Serialização de eventos: `EventTypeRegistry` + `JsonEventSerializer` (Infrastructure)

Mapa explícito nome-de-contrato -> tipo (`"wallet.account-opened"` -> `AccountOpened`), com
`System.Text.Json`. **Proibido** derivar do nome CLR ou usar `TypeNameHandling` (plano D6/R1).

- **Comportamento:** serializar e desserializar `AccountOpened` sem perda; nome de contrato
  estável mesmo se a classe C# for renomeada; tipo desconhecido falha alto e claro.
- **Teste (integração — sem container, `EventSerializationTests` em `IntegrationTests`):**
  `Serializar_AccountOpened_UsaONomeDeContratoEstavel`;
  `RoundTrip_AccountOpened_PreservaTodosOsCampos`;
  `Desserializar_ComNomeDeContratoDesconhecido_Lanca`.
- **Pronto quando:** os 3 testes passam.

### T15 [backend] — DDL do event store `001_event_store.sql` + `EventStoreSchema` (Infrastructure) — `[requer contêiner]`

> **Nota (2026-09-02, ADR-0011):** `001_event_store.sql` + `EventStoreSchema` (DDL idempotente
> aplicada na subida) foram substituídos por **migração EF Core versionada**
> (`Persistence/Migrations/InitialCreate`), aplicada por `dotnet ef database update` ou, em
> Development, opcionalmente na subida da Api. Teste equivalente agora em
> `EventStoreMigrationsTests`. Ver ADR-0011. O texto abaixo descreve a tarefa como
> originalmente entregue e não foi reescrito.

Tabela `wallet_events` conforme plano D6, PK `(aggregate_id, version)`, `global_seq bigserial`,
índice por `aggregate_id`, e revogação de `UPDATE`/`DELETE` para a role da aplicação (RN-3).

- **Comportamento:** o schema existe, é idempotente de aplicar e torna o passado fisicamente
  imutável.
- **Teste (integração, Testcontainers Postgres, `EventStoreSchemaTests`):**
  `Aplicar_DuasVezes_NaoFalha`;
  `Tabela_TemChavePrimariaAggregateIdVersion`;
  `Update_EmEventoJaGravado_EhRecusadoPelaPermissao` (RN-3 verificável).
- **Pronto quando:** os 3 testes passam com o container.

### T16 [backend] — Adapter `PostgresEventStore : IEventStore` (Infrastructure) — `[requer contêiner]`

Implementa `LoadAsync` (ordem por `version`) e `AppendAsync` (transação única, todos os
eventos ou nenhum; violação de PK -> `ConcurrencyException`; retorna os `EventEnvelope`
gravados). Único lugar do projeto que enxerga `Npgsql`. Classe `internal`.

- **Comportamento:** append-only com concorrência otimista (ADR-0007); RF-11 (nada
  parcialmente criado) garantido pela transação.
- **Teste (integração, `PostgresEventStoreTests`, base em `reference/integration.md`):**
  `Append_DepoisLoad_RetornaEventosNaOrdem`;
  `Append_DuasEscritasNaMesmaVersao_UmaFalhaPorConcorrencia`;
  `Append_ComFalhaNoMeio_NaoDeixaEventoParcial`;
  `Load_DeStreamInexistente_DevolveListaVazia`.
- **Pronto quando:** os 4 testes passam.

### T17 [backend] — Adapter `KafkaEventPublisher : IEventPublisher` (Infrastructure) — `[requer contêiner]`

Publica cada `EventEnvelope` no tópico `finagent.wallet.events` com **key = `aggregateId`**
(plano D7). Único lugar que enxerga `Confluent.Kafka`. Classe `internal`.

- **Comportamento:** a mensagem carrega `aggregateId`, `version`, `occurredAt`, `eventType`,
  `payload` — e **nenhum dado pessoal** (RF-10 / LGPD-9).
- **Teste (integração, Testcontainers Kafka, `KafkaEventPublisherTests`):**
  `Publish_AccountOpened_EntregaMensagemComKeyIgualAoAggregateId`;
  `Publish_AccountOpened_MensagemTemEventTypeEVersion`.
- **Pronto quando:** os 2 testes passam (consumidor de teste lendo o tópico, sem `Sleep` fixo).

### T18 [backend] — Projeção `LedgerProjection` + `AccountDocument` (Infrastructure) — `[requer contêiner]`

Copie de `reference/projection.md`, tratando **apenas** `AccountOpened` nesta feature
(o case de `TransactionPosted` chega com BL-1). Upsert idempotente guardado por
`Version < eventVersion`. `AccountDocument` ganha `OwnerId`.

- **Comportamento:** `AccountOpened` cria o documento com `BalanceCents = 0` e moeda BRL
  (RF-2); reprocessar a mesma mensagem não altera nada (Kafka é at-least-once).
- **Teste (integração, Testcontainers Mongo, `LedgerProjectionTests`):**
  `Projetar_AccountOpened_CriaDocumentoComSaldoZeroEMoedaBrl`;
  `Projetar_OMesmoAccountOpenedDuasVezes_NaoDuplicaNemAlteraODocumento`;
  `Projetar_AccountOpened_NaoGravaNenhumDadoPessoal`.
- **Pronto quando:** os 3 testes passam.

### T19 [backend] — Adapter `MongoWalletReadModel : IWalletReadModel` (Infrastructure) — `[requer contêiner]`

Lê o `AccountDocument` e devolve `WalletView`. Único lugar que enxerga `MongoDB.Driver` além
da projeção. Classe `internal`.

- **Comportamento:** id conhecido devolve a view; id nunca aberto devolve `null` — que é o que
  permite ao chamador recusar com "carteira inexistente" (RN-12).
- **Teste (integração, `MongoWalletReadModelTests`):**
  `GetById_DeCarteiraProjetada_DevolveWalletViewComSaldoZero`;
  `GetById_DeIdNuncaAberto_DevolveNull`.
- **Pronto quando:** os 2 testes passam.

### T20 [backend] — Consumidor `WalletProjectionWorker` (Infrastructure) — `[requer contêiner]`

`BackgroundService` que consome `finagent.wallet.events`, desserializa pelo registry (T14),
chama a projeção (T18) e faz commit **depois** de projetar (`enable.auto.commit=false`).

- **Comportamento:** um `AccountOpened` publicado no Kafka aparece no read model do Mongo sem
  intervenção.
- **Teste (integração ponta a ponta, Kafka + Mongo, `WalletProjectionWorkerTests`):**
  `AoConsumirAccountOpened_ReadModelPassaAConterACarteira` (poll de condição com timeout —
  **nunca** `Thread.Sleep`);
  `AoReentregarAMesmaMensagem_ReadModelPermaneceIgual`.
- **Pronto quando:** os 2 testes passam.

### T21 [backend] — `DependencyInjection` de Application e Infrastructure + adapters in-memory de dev (Infrastructure)

`AddWalletApplication()` registra `OpenWalletHandler`. `AddWalletInfrastructure(cfg)` registra
os adapters reais e, quando `Wallet:Store == "InMemory"`, registra
`InMemoryEventStore`/`InMemoryEventPublisher`/`InMemoryWalletReadModel` (plano D5 — conveniência
de execução local, nunca o default). Todos os adapters `internal`, expostos só via DI.

- **Comportamento:** a Api sobe e o fluxo de abertura funciona **sem Docker** com
  `Wallet:Store=InMemory`, e com Postgres/Kafka/Mongo no default.
- **Teste (integração, `DependencyInjectionTests` — sem container):**
  `AddWalletInfrastructure_ComStoreInMemory_ResolveTodasAsPortas`;
  `AddWalletApplication_ResolveOOpenWalletHandler`.
- **Pronto quando:** os 2 testes passam e nenhum adapter é `public`.

---

## Fase 4 — Api

### T22 [backend] — Endpoint `POST /wallets` + `OwnerContext` (Api) — RF-1, RF-4, RN-14

Minimal API sem corpo de request; o `ownerId` vem **só** do `OwnerContext` (header
`X-Owner-Id` na v1, plano D10 — nunca do body). `201 Created` + `Location` quando cria;
`200 OK` quando o titular já tinha carteira; `401` sem titular identificado. Substitui o
`MapGet("/")` do bootstrap e compõe `AddWalletApplication()` + `AddWalletInfrastructure()`.
OpenAPI pelo `AddOpenApi()`/`MapOpenApi()` nativos — **Swashbuckle é proibido**.

- **Comportamento:** o titular abre a própria carteira e recebe o identificador; repetir o
  pedido devolve a mesma carteira; não existe como abrir carteira de terceiro.
- **Teste (integração de API, `WebApplicationFactory` + `Wallet:Store=InMemory`, `AbrirCarteiraEndpointTests`):**
  `Post_Wallets_ComTitularNovo_Retorna201ComWalletIdEMoedaBrl`;
  `Post_Wallets_RepetidoParaOMesmoTitular_Retorna200ComOMesmoWalletId`;
  `Post_Wallets_SemTitularIdentificado_Retorna401`;
  `Post_Wallets_ComOwnerIdNoCorpo_IgnoraOCorpoEUsaOContexto` (RN-14 — o teste que impede o
  bug de abrir carteira alheia).
- **Pronto quando:** os 4 testes passam e o request DTO **não** tem campo `ownerId`.

### T23 [backend] — Mapeamento de erro para ProblemDetails (Api) — RN-12, RN-9

Handler de exceção único: `WalletNotFoundException` -> `404` com motivo
`"carteira inexistente"`; `DomainException` de moeda -> `422` com `"moeda divergente"`;
`DomainException` genérica -> `422`; `ConcurrencyException` -> `409`. Nenhum stack trace nem
dado sensível no corpo (LGPD-9).

- **Comportamento:** a recusa chega ao chamador com motivo explícito e estável — é o
  vocabulário que BL-1, BL-2, BL-3, BL-6 e BL-7 vão reutilizar.
- **Teste (integração de API, `ProblemDetailsMappingTests`, com um endpoint de teste que
  lança cada exceção):**
  `WalletNotFound_Vira404ComMotivoCarteiraInexistente`;
  `DomainExceptionDeMoeda_Vira422ComMotivoMoedaDivergente`;
  `ConcurrencyException_Vira409`;
  `RespostaDeErro_NaoVazaStackTrace`.
- **Pronto quando:** os 4 testes passam.

---

## Fase 5 — Camada agêntica

### T24 [backend] — Tool MCP `abrir_carteira` (`FinAgent.Ai.McpServer`) — spec seção 8

> **Bloqueada pela pendência P2 do plano** (versão do SDK MCP não fixada em ADR). T1–T23 não
> dependem dela. Não existe template canônico de MCP nas skills — documente o que fizer.

Tool sem argumento de titular (o `ownerId` vem do contexto da conversa, RN-14), sem passo de
confirmação (RN-13: confirmação é só para o que move dinheiro), chamando `POST /wallets` por
HTTP. A tool **não** referencia Domain, Application nem banco.

- **Comportamento:** "me abre uma carteira" devolve o identificador; pedir de novo devolve a
  mesma carteira, sem criar outra.
- **Teste (integração, `AbrirCarteiraToolTests`, com a Wallet Api em `WebApplicationFactory` e
  `Wallet:Store=InMemory`):**
  `Tool_AbrirCarteira_DevolveOWalletIdDoTitularDaConversa`;
  `Tool_AbrirCarteira_ChamadaDuasVezes_DevolveOMesmoWalletId`;
  `Tool_AbrirCarteira_NaoAceitaTitularComoArgumento` (RN-14 — o LLM não pode escolher de quem
  é a carteira).
- **Pronto quando:** os 3 testes passam e a versão do SDK MCP está **fixada** no `.csproj`.

---

## Fechamento da feature

Depois de T24, antes de dar BL-5 por entregue:

1. `dotnet build` limpo (warnings-as-errors) e `dotnet test` verde — com Docker para a suíte
   completa e sem Docker para a suíte de unidade.
2. Rodar o `code-reviewer` sobre as mudanças.
3. Rodar o `wallet-specialist` para conferir por ID: RN-2, RN-3, RN-8, RN-9, RN-12, RN-13,
   RN-14, RN-15 (RN-1 não é exercitada aqui — a abertura não movimenta dinheiro).
4. Percorrer os 10 critérios de aceite da spec seção 11 e apontar o teste que prova cada um.
5. Atualizar `docs/backlog.md`: BL-5 de "spec escrita" para "entregue".
6. Follow-up do plano D4: atualizar
   `.claude/skills/dotnet-event-sourcing-cqrs/reference/command-handler.md` com o `EventEnvelope`.
