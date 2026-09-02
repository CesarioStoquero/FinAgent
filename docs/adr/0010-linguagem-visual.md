# ADR-0010: Linguagem visual do frontend

- **Status:** Proposto  ·  (vira **Aceito** quando uma direção é escolhida via `/definir-design`)
- **Data:** [A DEFINIR]
- **Decisores:** [A DEFINIR]

## Contexto
O FinAgent tem a stack de frontend fixada (Angular 22, ADR-0009) e as skills de ARQUITETURA
(`angular-architecture`) e CÓDIGO (`angular-conventions`). Faltava a terceira perna: a
LINGUAGEM VISUAL — o que faz a UI ser bonita, moderna e reconhecível como FinAgent, e não um
template genérico "com cara de IA". Sem uma direção gravada, cada tela reinventaria
cor/fonte/spacing no olho — inconsistência e slop garantidos.

Este ADR é o ponto onde a escolha visual é REGISTRADA e vira lei ✓ do harness. Ele é
preenchido pelo fluxo `/definir-design`: catálogo de direções → 2 previews em 2 links →
escolha do usuário → este ADR + `src/styles/tokens.css`.

## Decisão
> ⚠ **A DEFINIR.** Nenhuma direção foi ESCOLHIDA ainda. O catálogo
> (`.claude/skills/frontend-design/reference/catalog.md`) já tem 3 direções curadas
> (`radix-precise` · MIT, `carbon-dark` · Apache-2.0, `editorial-sand` · MIT). O primeiro
> `/definir-design` oferece 2 delas em 2 links; a escolhida é registrada aqui e vira lei ✓.

Quando decidido, registre aqui:
- **Direção escolhida:** `<id>` (de `frontend-design/reference/directions/<id>.md`).
- **Tokens materializados em:** `src/styles/tokens.css` (fonte única).
- **Fonte destilada + licença:** <design system / lib aberta> · <MIT / Apache-2.0 / …>.
- **Personalidade em uma frase:** <…>.

## Consequências
- (+) 100 telas coerentes por construção: todo componente lê `tokens.css`; trocar de direção é
  barato (troca os tokens, não as telas).
- (+) O `frontend-engineer` para de escolher visual no olho — some a "cara de IA".
- (−) Exige curar o catálogo (pesquisa de fontes SEM copyright) antes do primeiro uso.
- (−) Uma direção fixada é uma restrição: variações fortes exigem novo `/definir-design`.

## Alternativas consideradas
- **Sem direção fixada (cada tela decide):** descartado — é exatamente a fonte da
  inconsistência e do slop que este ADR existe para evitar.
- **Clonar o design de um produto de referência:** descartado — violação de copyright; o
  catálogo destila princípios/tokens de fontes abertas, nunca o markup/jeitão de um produto.
- **Direção perdedora do `/definir-design`:** anote aqui a que perdeu e por quê (registra que
  a escolha foi deliberada, não default).
