# Backlog do FinAgent

> Derivado por `/derivar-backlog` em 2026-09-02 (rodada **completa**).
> Fonte: `docs/business-rules.md` + `docs/modules/wallet/business-rules.md` +
> `docs/modules/compliance/business-rules.md` + `docs/adr/` + inventário de `src/`, `tests/`, `.github/`.
> Item aqui é **candidato**, não compromisso. Vira trabalho quando você roda a entrada dele.

> **Substitui a rodada escopada (só ADR-0001).** IDs preservados: BL-2 e BL-3 seguem iguais;
> **BL-1** era o genérico "registrar uma movimentação" e foi **estreitado para depositar** —
> com RN-11/RN-12 em escopo, a movimentação tem forma concreta e as duas pendências antigas
> se resolveram sozinhas.

## Legenda

- **Tipo:** `capacidade` (comportamento — vira spec via `/nova-spec`) · `chore` (scaffold,
  infra, CI — vai direto ao `software-engineer`, sem spec)
- **Estado:** `pendente` · `spec escrita` · `em entrega` · `entregue`
- **Regra ✓** é lei; **regra ⚠** ainda depende da sua decisão (o item nasce com pendência).

## Itens

### BL-4 — Solution .NET e projetos das camadas

- **Tipo:** chore · **Módulo:** — · **Estado:** pendente
- **O quê:** criar o `.sln` e os `.csproj` de `Wallet.Domain`, `Wallet.Application`,
  `Wallet.Infrastructure`, `Wallet.Api`, `Ai.McpServer`, `Ai.Agent` + os dois projetos de
  teste, com as referências apontando pra dentro e o `Directory.Build.props` já existente.
- **Por quê:** `src/` e `tests/` só têm `_README.md` — não há `.sln` nem `.csproj`. Nenhum
  item de capacidade é executável, e o hook de build (`verify-build.ps1`) fica inerte.
- **Satisfaz:** — (chore não satisfaz regra de negócio; nasce do inventário)
- **Restringido por:** ADR-0003 (dependências apontam pra dentro), ADR-0009 (`net10.0`, C# 14,
  xUnit v3 + AwesomeAssertions, OpenAPI nativo — sem Swashbuckle)
- **Depende de:** nada
- **Decisões abertas:** nenhuma
- **Entrada:** `backend-engineer` direto — o template canônico existe em `reference/project-layout.md`, seção "Bootstrap da solution"

### BL-5 — Abrir uma carteira

- **Tipo:** capacidade · **Módulo:** wallet · **Estado:** spec escrita
- **O quê:** o titular passa a ter uma carteira própria, identificada, apta a receber e
  movimentar valor. Antes disso nenhuma operação é possível.
- **Por quê:** RN-12 torna toda operação dependente de uma carteira existente. É a raiz de
  BL-1, BL-6 e BL-7.
- **Satisfaz:** RN-12 ✓
- **Restringido por:** ADR-0008 (a carteira é uma conta do tipo `Customer` no ledger de
  partida dobrada), ADR-0001, RN-9 ✓ (moeda única por operação)
- **Depende de:** BL-4
- **Decisões abertas:** nenhuma — as 26 pendências foram decididas em 2026-09-02
- **Spec:** `docs/specs/abrir-carteira/spec.md` — **Validada** em 2026-09-02
- **Entrada:** `/deliver-feature docs/specs/abrir-carteira/spec.md`

### BL-1 — Depositar dinheiro na carteira

- **Tipo:** capacidade · **Módulo:** wallet · **Estado:** spec escrita
- **O quê:** o titular põe valor na carteira; o sistema registra o movimento como fato
  definitivo, que não é alterado nem apagado depois.
- **Por quê:** é a primeira entrada de dinheiro no sistema — sem ela não há saldo (BL-2)
  nem histórico (BL-3) para observar.
- **Satisfaz:** RN-11 ✓ (valor > 0), RN-5 ✓ (movimento sempre registrado), RN-3 ✓ (registro
  imutável), RN-1 ✓ (a entrada vem da contrapartida do sistema, soma zero)
- **Restringido por:** ADR-0001, ADR-0008, RN-8 ✓ (centavos, `long`), RN-9 ✓
- **Depende de:** BL-5
- **Decisões abertas:** nenhuma — as 26 pendências foram decididas em 2026-09-02
- **Spec:** `docs/specs/depositar-na-carteira/spec.md` — **Validada** em 2026-09-02
- **Entrada:** `/deliver-feature docs/specs/depositar-na-carteira/spec.md`

### BL-2 — Consultar o saldo da carteira

- **Tipo:** capacidade · **Módulo:** wallet · **Estado:** spec escrita
- **O quê:** o titular pergunta quanto tem e recebe um valor calculado a partir dos
  movimentos registrados — nunca um número guardado à parte.
- **Por quê:** é a pergunta mais frequente do produto e a prova viva de RN-2: saldo vindo de
  coluna mutável significa ADR-0001 violada.
- **Satisfaz:** RN-2 ✓
- **Restringido por:** ADR-0001, ADR-0002 (leitura pelo read model), RN-8 ✓
- **Depende de:** BL-1
- **Decisões abertas:** nenhuma — as 26 pendências foram decididas em 2026-09-02
- **Spec:** `docs/specs/consultar-saldo/spec.md` — **Validada** em 2026-09-02
- **Entrada:** `/deliver-feature docs/specs/consultar-saldo/spec.md`

### BL-10 — Pipeline de CI (build + testes)

- **Tipo:** chore · **Módulo:** — · **Estado:** pendente
- **O quê:** workflow do GitHub Actions rodando `dotnet build` (warnings-as-errors) e
  `dotnet test` a cada push/PR.
- **Por quê:** `.github/` está vazio — o workflow antigo foi removido por apontar `.NET 8`
  (commit `4ec7fbd`) e nada o substituiu. Hoje só o hook local protege o build.
- **Satisfaz:** — (nasce do inventário)
- **Restringido por:** ADR-0009 (SDK `net10.0`, xUnit v3)
- **Depende de:** BL-4 e BL-1 (CI sem código nem teste é ruído)
- **Decisões abertas:** nenhuma
- **Entrada:** `software-engineer` direto (chore de CI; não tem template canônico ainda)

### BL-6 — Sacar dinheiro da carteira

- **Tipo:** capacidade · **Módulo:** wallet · **Estado:** spec escrita
- **O quê:** o titular retira valor da carteira; a retirada é recusada se o valor não for
  positivo ou se deixaria o saldo negativo.
- **Por quê:** fecha o par depósito/saque e é onde a regra de recusa (RN-10) fica observável.
- **Satisfaz:** RN-10 ✓ (saldo não-negativo), RN-11 ✓ (valor > 0), RN-5 ✓, RN-3 ✓, RN-1 ✓
- **Restringido por:** ADR-0001, ADR-0008, RN-8 ✓, RN-9 ✓
- **Depende de:** BL-1 (precisa haver saldo para sacar)
- **Decisões abertas:** nenhuma — as 26 pendências foram decididas em 2026-09-02
- **Spec:** `docs/specs/sacar-da-carteira/spec.md` — **Validada** em 2026-09-02
- **Entrada:** `/deliver-feature docs/specs/sacar-da-carteira/spec.md`

### BL-3 — Consultar o extrato da carteira

- **Tipo:** capacidade · **Módulo:** wallet · **Estado:** spec escrita
- **O quê:** o titular vê a sequência de movimentos que explica o saldo atual, do primeiro
  ao último, sem lacunas.
- **Por quê:** RN-6 exige que cada centavo seja explicável percorrendo os registros. Sem esta
  capacidade a auditabilidade prometida pela ADR-0001 não é observável por ninguém.
- **Satisfaz:** RN-6 ✓
- **Restringido por:** ADR-0001, ADR-0002, ADR-0005 (extrato vem do read model)
- **Depende de:** BL-1
- **Decisões abertas:** nenhuma — as 26 pendências foram decididas em 2026-09-02
- **Spec:** `docs/specs/consultar-extrato/spec.md` — **Validada** em 2026-09-02
- **Entrada:** `/deliver-feature docs/specs/consultar-extrato/spec.md`

### BL-7 — Estornar uma transação

- **Tipo:** capacidade · **Módulo:** wallet · **Estado:** spec escrita
- **O quê:** corrigir um movimento errado criando um novo movimento que o anula, deixando
  os dois visíveis e encadeados no histórico. O movimento original nunca é editado.
- **Por quê:** é a única forma de correção permitida (RN-4) e a prova de que RN-3 não é
  slogan. Sem ela, todo erro operacional vira pedido de editar o passado.
- **Satisfaz:** RN-4 ✓ (correção só por estorno), RN-3 ✓, RN-1 ✓ (o estorno também fecha em zero)
- **Restringido por:** ADR-0008, ADR-0001
- **Depende de:** BL-1
- **Decisões abertas:** nenhuma — as 26 pendências foram decididas em 2026-09-02
- **Spec:** `docs/specs/estornar-transacao/spec.md` — **Validada** em 2026-09-02
- **Entrada:** `/deliver-feature docs/specs/estornar-transacao/spec.md`

### BL-8 — Transação em duas fases (pendente → postada)

- **Tipo:** capacidade · **Módulo:** wallet · **Estado:** pendente · **DEPENDE DE DECISÃO (⚠)**
- **O quê:** uma transação nasce pendente (ainda ajustável), e só ao ser postada vira fato
  imutável. Falhas vão para arquivada, nunca apagadas.
- **Por quê:** espelha prazos bancários reais (ACH/TED levam dias). Mas **RN-7 está ⚠**: é
  princípio importado (Modern Treasury), não decisão do FinAgent. Hoje a transação nasce postada.
- **Satisfaz:** RN-7 ⚠ — não é lei
- **Restringido por:** ADR-0001, ADR-0008
- **Depende de:** BL-1, BL-6 · e da sua decisão sobre RN-7
- **Decisões abertas:**
  `[NEEDS CLARIFICATION: adotar RN-7? Só faz sentido se o FinAgent precisar de liquidação em duas fases. Enquanto ⚠, este item NÃO entra em entrega]`
- **Entrada:** decidir RN-7 primeiro; se virar ✓, `/nova-spec "transação em duas fases"`

### BL-9 — Eliminar os dados pessoais de um titular

- **Tipo:** capacidade · **Módulo:** compliance · **Estado:** pendente · **DEPENDE DE DECISÃO (⚠)**
- **O quê:** a pedido do titular, o dado pessoal dele se torna permanentemente inacessível —
  em toda a plataforma, inclusive em telas e consultas — sem que nenhum lançamento
  financeiro seja apagado ou alterado.
- **Por quê:** é o conflito central do projeto (ledger imutável × direito de eliminação) e a
  única capacidade que resolve LGPD-2. Mas **todas as LGPD-x estão ⚠** — falta ADR e
  validação jurídica.
- **Satisfaz:** LGPD-2 ⚠, LGPD-6 ⚠, LGPD-7 ⚠ (revogar consentimento dispara a eliminação) —
  nenhuma é lei ainda
- **Restringido por:** LGPD-4 ⚠ (o lançamento financeiro é retido; elimina-se o PII
  vinculável), LGPD-3 ⚠ (crypto-shredding), RN-3 ✓ (evento não muda), ADR-0001
- **Depende de:** sua decisão sobre as LGPD-x · e de existir PII no sistema (ver Lacunas)
- **Decisões abertas:**
  `[NEEDS CLARIFICATION: confirmar as LGPD-1..9 como lei do projeto (vira ADR) antes de qualquer entrega?]`
- **Entrada:** confirmar as LGPD-x primeiro; se virarem ✓, `/nova-spec "eliminar os dados pessoais de um titular"`

## Ordem sugerida

1. **BL-4** (chore) — sem solution, nada roda.
2. **BL-5** — RN-12 faz todo o resto depender da carteira aberta.
3. **BL-1** — primeira entrada de dinheiro; destrava BL-2, BL-3, BL-6, BL-7.
4. **BL-2** — menor esforço sobre BL-1, entrega o valor mais pedido.
5. **BL-10** (chore) — agora existe código e teste para a CI proteger.
6. **BL-6** — fecha o par depósito/saque e exercita a recusa (RN-10).
7. **BL-3** — fecha a promessa de auditabilidade da ADR-0001.
8. **BL-7** — correção por estorno.
9. **BL-8**, **BL-9** — só depois de você decidir RN-7 e as LGPD-x.

> Critério do ranking: dependência primeiro (o que destrava mais), valor depois.

## Bloqueios ativos

| Bloqueio | Afeta | Situação |
|----------|-------|----------|
| RN-7 segue ⚠ e **não foi adotada na v1** (decisão 2026-09-02) | BL-8 | fora da v1: a transação nasce postada. BL-8 só volta se o FinAgent ganhar liquidação em duas fases |
| LGPD-1..9 todas ⚠ **e a v1 não coleta PII** (decisão 2026-09-02) | BL-9 | fora da v1: sem PII no sistema, não há o que eliminar. BL-9 volta junto com o cadastro de titular |
| ADR-0010 com Status "Proposto" | qualquer item de frontend | rodar `/definir-design` — hoje nenhum item do backlog é de frontend (ver Lacunas) |

## Lacunas do catálogo (regra faltando)

O que o projeto claramente PRETENDE fazer mas nenhuma regra de negócio sustenta. Não vira
item (invariante 6 — não se inventa regra); vira decisão sua: escrever a regra em
`docs/business-rules.md` ou aceitar que a capacidade não existe.

| Lacuna | Evidência de que se pretende | O que falta |
|--------|------------------------------|-------------|
| ~~Operar a carteira por linguagem natural~~ | — | **RESOLVIDA em 2026-09-02:** virou **RN-13** ✓ (agente consulta livre; movimento de dinheiro exige confirmação explícita; estorno não é operável pelo agente) |
| **Cadastrar um titular (dado pessoal)** | as LGPD-1..9 pressupõem um titular com nome/CPF/e-mail | **adiada, não resolvida:** a v1 decidiu não coletar PII (titular é identificador opaco), o que mantém LGPD-1 satisfeita por construção. A lacuna volta no dia em que o produto precisar de cadastro — e aí as LGPD-x ⚠ precisam virar ✓ antes |
| **Qualquer tela** | ADR-0010, skills `frontend-design`/`angular-*`, agentes `designer` e `frontend-engineer` | nenhuma regra de negócio gera interface. O frontend segue sem um único item candidato |

## Fora do backlog (e por quê)

Rastreabilidade deliberada: o que foi lido e **não** virou item.

| ADR / Regra | Por que não virou item |
|-------------|------------------------|
| RN-3 ✓ (imutabilidade) | invariante, não comportamento — ninguém "usa" imutabilidade. Vira restrição de BL-1/6/7 e critério de aceite |
| RN-8 ✓ (centavos) | restringe COMO todo valor é representado; vale para todo item |
| RN-9 ✓ (moeda única) | restringe COMO a operação acontece. "Converter moeda" seria regra NOVA — não invento (invariante 6) |
| LGPD-1, 3, 4, 5, 8, 9 ⚠ | dizem ONDE o PII mora e COMO é protegido — restrição de desenho, não capacidade. Entram no `plan.md` de qualquer item com PII |
| ADR-0001, 0002, 0003, 0004, 0005, 0007, 0009 | estruturais: sem comportamento observável. Restringem TODO item |
| ADR-0006 (camada agêntica) | estrutural; a regra que faltava virou **RN-13** ✓ — a camada agêntica é atributo de cada item (seção 8 das specs), não item próprio |
| ADR-0008 (partida dobrada) | modelo contábil: restringe BL-1/5/6/7, não é trabalho isolado |
| ADR-0010 (linguagem visual) | decisão pendente sua — virou Bloqueio ativo |
