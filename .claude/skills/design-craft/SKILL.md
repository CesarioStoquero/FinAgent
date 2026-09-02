---
name: design-craft
description: Conhecimento de designer de produto sênior — UX, heurísticas de usabilidade, arquitetura de informação, hierarquia visual, tipografia, sistemas de cor e acessibilidade (WCAG 2.2). Reaproveitável entre projetos, independente de framework. Use ao projetar QUALQUER interface, decidir fluxo/layout/estados, avaliar usabilidade ou escolher direção visual.
---

# design-craft — o que um designer de produto sênior sabe

Skill de CRAFT, agnóstica de projeto (reutilize em qualquer projeto; o que é específico do
FinAgent mora em `frontend-design`). Responde "isto é bom design de PRODUTO?" — não só bonito,
mas usável, acessível e adequado ao contexto.

## Princípio-guia: adequação ao contexto (quando inovar, quando ser sóbrio)

Um sênior não aplica o mesmo "molho" em tudo. Decida a AMBIÇÃO visual pelo contexto:
- **Contexto de PRODUTO** (dashboard, ferramenta, dados densos, uso repetido): a meta é
  clareza e velocidade. Sobriedade > novidade. Inove no fluxo e na densidade, não no enfeite.
- **Contexto de MARCA/marketing** (landing, onboarding, primeira impressão): a meta é memória
  e emoção. Aqui a expressão e a inovação visual ganham peso.
- Um mesmo projeto tem os dois. Não trate o app inteiro como landing, nem a landing como
  planilha. (é o erro nº 1 do júnior.)

## Invariantes (NUNCA quebre)

1. SEMPRE forma segue função: a estrutura (fluxo, IA, hierarquia) vem antes da pele (cor,
   tipo). Nunca escolha cor antes de entender a TAREFA do usuário.
2. SEMPRE acessível por padrão (WCAG 2.2 AA — abra `reference/accessibility-wcag.md`). Não é
   opcional nem "fase 2".
3. SEMPRE um elemento dominante por tela; o resto subordinado. Sem hierarquia não há design.
4. NUNCA "data slop"/enfeite: cada elemento ganha o lugar (heurística 8 — minimalista).
5. SEMPRE modele os ESTADOS (vazio, carregando, erro, sucesso, denso) — não só o feliz.
6. NUNCA destile visual de fonte com copyright: princípios/tokens de fonte aberta, nunca o
   markup ou o "jeitão" distintivo de um produto real.

## reference/ (abra sob demanda — progressive disclosure)

| Vou decidir…                              | Abra |
|-------------------------------------------|------|
| Se a tela/fluxo é usável (avaliar)        | `reference/ux-heuristics.md` (Nielsen 10) |
| Pesquisa → IA → fluxo (antes de desenhar) | `reference/ux-process.md` |
| O que salta primeiro (peso, ordem)        | `reference/visual-hierarchy.md` |
| Fonte, escala tipográfica, ritmo          | `reference/typography.md` |
| Paleta, papéis de cor, oklch, escalas     | `reference/color-systems.md` |
| Acessibilidade concreta (contraste, foco) | `reference/accessibility-wcag.md` |
| Estados de componente e de tela           | `reference/interaction-states.md` |
| Movimento com propósito                   | `reference/motion.md` |

## Checklist do sênior (antes de dar a tela por pronta)

- [ ] Entendi a TAREFA do usuário e o contexto (produto vs marca) antes da pele?
- [ ] Passa nas 10 heurísticas de Nielsen (rodou `ux-heuristics.md`)?
- [ ] Hierarquia: um dominante, resto subordinado?
- [ ] Todos os estados modelados (vazio/carregando/erro/sucesso)?
- [ ] AA: contraste, foco visível, alvo ≥ 24px (conforto 44px), teclado?
- [ ] Adequação: inovei onde importa e fui sóbrio onde importa?
