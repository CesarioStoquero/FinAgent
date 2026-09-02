---
name: derivar-backlog
description: Deriva o BACKLOG de capacidades do FinAgent a partir das regras de negócio (RN-x/LGPD-x), dos ADRs e do que já existe em src/, e escreve docs/backlog.md — cada item com as regras que satisfaz, os ADRs que o restringem e o comando de entrada. Use quando a pergunta for "o que falta construir?", ao iniciar o projeto, ou ao abrir um módulo novo. Invocável como /derivar-backlog.
---

# derivar-backlog — de conhecimento para trabalho

O harness tem CONHECIMENTO (regras, ADRs) e um PIPELINE de entrega (`/nova-spec` →
`/deliver-feature`), mas nada que ligue os dois: não há como responder "o que falta
construir?". Este fluxo produz essa ponte — `docs/backlog.md`, uma lista de candidatos
ranqueada. Cada item vira depois um `/nova-spec`.

Backlog é **candidato**, não compromisso. Você propõe; o usuário decide o que entra.

## Invariantes (NUNCA quebre)

1. **Item de backlog = capacidade observável** ("consultar o saldo da carteira"). NUNCA um
   artefato técnico ("criar o agregado Account", "subir o Kafka") e NUNCA uma camada.
2. **ADR não é fonte de capacidade — é RESTRIÇÃO.** Nenhum item nasce de um ADR estrutural
   (hexagonal, Kafka, Mongo, stack). A fonte de capacidade são as REGRAS DE NEGÓCIO; o ADR
   entra em "Restringido por". ADR que não gerou item vai para a tabela "Fora do backlog",
   com o motivo — é assim que a rastreabilidade ADR→spec acontece aqui.
3. **Regra ✓ pode virar item; regra ⚠ não vira lei.** Item derivado de uma ⚠ nasce marcado
   "depende de decisão" e carrega a pendência.
4. **Verifique antes de propor.** Use Grep/Glob em `src/`, `tests/` e `docs/specs/` para não
   colocar no backlog o que já existe ou já tem spec. Greenfield? diga que está tudo por fazer.
5. **Chore é item de primeira classe, mas não vira spec.** Scaffold da solution, CI, infra:
   `Tipo: chore`, entra no backlog (porque bloqueia), e vai DIRETO ao engenheiro — não passa
   pelo `/nova-spec`, que é para comportamento.
   **A fonte do chore é o INVENTÁRIO (passo 4) — não as regras, não os ADRs.** A invariante 2
   vale só para CAPACIDADE. A pergunta que gera chore é: *"o que precisa existir para QUALQUER
   item rodar?"*. Um ADR pode dizer COMO o chore é feito (ADR-0009 fixa a versão), nunca que
   ele existe.
6. **NUNCA invente regra de negócio.** Lacuna vira `[NEEDS CLARIFICATION]` no item, e a regra
   nova só existe depois que o usuário decidir e ela entrar em `docs/business-rules.md`.
7. **Não reescreva o backlog do zero** se ele já existe: atualize os estados e some os itens
   novos, preservando os IDs `BL-x` (eles são citados fora do arquivo).

## Procedimento (ordem fixa)

1. **Catálogo.** Leia `docs/business-rules.md` — é o índice, com o status ✓/⚠ de cada regra.
2. **Módulos.** Para cada módulo em `docs/modules/README.md`, abra o `business-rules.md` dele
   e extraia as **capacidades implícitas** nas regras. Pergunta-guia: *"para esta regra existir
   no mundo, que ação o usuário ou o sistema precisa poder fazer?"*
   (ex.: RN-12 "só se opera conta já aberta" → a capacidade "abrir uma carteira").
3. **ADRs.** Percorra `docs/adr/` só para (a) mapear a restrição de cada item e (b) captar
   ADR com Status ainda não "Aceito", que vira **bloqueio** (ex.: ADR-0010 "Proposto" bloqueia
   qualquer item de frontend até `/definir-design`).
4. **Inventário — e é aqui que os CHORES nascem.** Grep/Glob em `src/`, `tests/`, `docs/specs/`:
   o que já existe sai do backlog (ou entra com estado `entregue`). Depois liste o que está
   FALTANDO para qualquer item ser executável — solution/projetos `.sln`/`.csproj`, CI, infra
   local, tokens de design. Cada lacuna vira um item `Tipo: chore`, ranqueado ANTES das
   capacidades que ele bloqueia. Nada de empurrar chore para "Bloqueios ativos": lá só entra o
   que NÃO é trabalho seu (ex.: uma decisão sua ainda pendente).
5. **Ranquear** por dependência primeiro (o que destrava mais itens), valor depois. Chore que
   bloqueia tudo vem antes de qualquer capacidade.
6. **Escrever** `docs/backlog.md` copiando a estrutura de `reference/backlog-template.md`.
7. **Reportar** ao usuário: quantos itens, o próximo sugerido e o comando literal de entrada.

## Tabela de decisão (o que abrir)

| Vou… | Abra |
|------|------|
| escrever ou atualizar o backlog | `reference/backlog-template.md` (estrutura canônica) |
| saber o status ✓/⚠ de uma regra | `docs/business-rules.md` |
| entender o que a regra exige | `docs/modules/<modulo>/business-rules.md` |
| saber que restrição se aplica | `docs/adr/` (o ADR citado pela regra) |
| descobrir os módulos existentes | `docs/modules/README.md` |

## Saída e próximo passo

Arquivo: `docs/backlog.md`. Ao terminar, diga o próximo passo literal:
- capacidade → `/nova-spec "<frase do item>"` (o `business-analyst` escreve a spec)
- TODAS as capacidades de uma vez → ofereça o **modo lote** do `nova-spec`: uma só chamada ao
  `business-analyst` gera a spec de cada item não-chore e não-⚠, e devolve as pendências agrupadas
- chore → "peça ao `software-engineer` para planejar o chore BL-x" (não tem spec)
