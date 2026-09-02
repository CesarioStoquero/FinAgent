---
name: frontend-design
description: Linguagem visual do frontend Angular do FinAgent — como fazer uma UI moderna, bonita e SEM cara de IA. Use ao criar QUALQUER tela, página ou componente visual, ao escolher cor/tipografia/espaçamento, ou ao gerar previews de direção visual. Trabalha junto com angular-architecture (estrutura) e angular-conventions (código).
---

# Design de frontend (FinAgent) — a linguagem visual

**Stack fixada (ADR-0009):** Angular 22 (standalone + signals). **Linguagem visual (ADR-0010):**
UMA direção escolhida vira lei ✓; os tokens moram em `src/styles/tokens.css` (fonte única).
NUNCA invente cor/fonte/spacing fora dos tokens.

Esta skill responde "a tela está BONITA e é do FinAgent — não um template genérico de IA?".
A `angular-architecture` responde "está bem ESTRUTURADA?" e a `angular-conventions`, "o código
está limpo?". As três juntas fazem a tela.

## Como isto funciona (o fluxo do harness)

1. Uma vez por projeto (ou ao revisar a marca): `/definir-design` explora o CATÁLOGO de
   direções, gera 2 previews em 2 links, você escolhe, e a escolha é gravada em **ADR-0010**
   + `src/styles/tokens.css`. É o "grava a escolha" do harness.
2. Daí em diante: TODA tela consome esta skill + os tokens gravados. O `frontend-engineer`
   nunca escolhe cor/fonte no olho — ele lê `tokens.css`.

## Invariantes (NUNCA quebre)

1. NUNCA cor/fonte/raio/spacing hard-coded no componente. SEMPRE via `var(--token)` de
   `src/styles/tokens.css`. Token novo → primeiro no tokens.css, depois no componente.
2. NUNCA as marcas de "cara de IA" (ABRA e siga `reference/anti-ai-slop.md`): Inter/Roboto/
   Arial no corpo; gradiente-blob de fundo; card com canto muito arredondado + borda-accent
   à esquerda; emoji como ícone; número em fonte proporcional.
3. SEMPRE número/dinheiro/id em fonte MONO com `font-variant-numeric: tabular-nums`.
   Dinheiro exibido em BRL; enviado ao backend em centavos (ver angular-conventions).
4. SEMPRE ícone como SVG inline (stroke, grid 20/24px, um estilo só). NUNCA emoji/dingbat.
5. SEMPRE contraste AA (texto normal ≥ 4.5:1, grande ≥ 3:1) e alvo de toque ≥ 44px.
6. SEMPRE respeite a DIREÇÃO gravada no ADR-0010. Trocar de direção é decisão de projeto
   (novo `/definir-design`), não escolha de componente.

## Escolher a direção (heurística: contexto → direção)

Quando `/definir-design` roda, cruze o contexto do projeto com o CATÁLOGO
(`reference/catalog.md`). Sinais e a direção que costumam pedir:

| Sinal no projeto/spec | Empurra para… |
|-----------------------|----------------|
| dados densos, tabelas, números, confiança | fintech precisa / data-first |
| AI-first, MCP, event sourcing, ferramenta técnica | escuro técnico / terminal-like |
| marca forte, marketing, primeira impressão | editorial / expressivo |
| marca, marketing, autoral | `editorial-sand` (editorial & expressivo) |

Escolha as **2 direções mais aderentes** (não 5 variações da mesma) e gere previews.
O catálogo (`reference/catalog.md`) tem 3 direções curadas (`radix-precise`, `carbon-dark`,
`editorial-sand`), todas de fontes SEM copyright. Ampliar = clonar `directions/_TEMPLATE.md`.
Se algum dia o catálogo estiver vazio, `/definir-design` não inventa: para e pede curadoria.

## reference/ (abra sob demanda — progressive disclosure)

| Vou fazer…                                | Abra |
|-------------------------------------------|------|
| Escolher/entender uma direção do catálogo | `reference/catalog.md` (índice) |
| Registrar uma direção nova no catálogo    | `reference/directions/_TEMPLATE.md` |
| Evitar as marcas de "cara de IA"          | `reference/anti-ai-slop.md` |
| Gerar `src/styles/tokens.css` da escolha  | `reference/design-tokens.md` |
| Gerar os 2 previews em 2 links            | `reference/preview-artboard.md` |

## Checklist antes de concluir uma tela

- [ ] Zero cor/fonte/spacing hard-coded — tudo em token de `tokens.css`?
- [ ] Passou no `reference/anti-ai-slop.md` (nenhuma marca de IA)?
- [ ] Números/ids em mono + tabular-nums; dinheiro em BRL?
- [ ] Ícones SVG inline; contraste AA; alvo ≥ 44px?
- [ ] Coerente com a direção do ADR-0010 (não "melhorou" por conta própria)?
