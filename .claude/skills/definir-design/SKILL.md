---
name: definir-design
description: Define a LINGUAGEM VISUAL do FinAgent — explora o catálogo de direções, gera 2 previews em 2 links para o usuário escolher, e grava a escolha em ADR-0010 + src/styles/tokens.css. Use quando for decidir o visual do projeto (ou revisar a marca), ex.: "vamos definir o design", "escolher a identidade visual". Invocável como /definir-design.
---

# definir-design — porta de entrada do VISUAL do FinAgent

Você (agente principal) orquestra a escolha da linguagem visual. Consome a skill
`frontend-design`. NÃO implementa tela Angular aqui — isso é do `frontend-engineer` depois,
contra os tokens gravados. Pode delegar a MONTAGEM dos previews ao `designer`.

## Invariantes (NUNCA quebre)

1. NUNCA invente direção fora do CATÁLOGO (`frontend-design/reference/catalog.md`). Catálogo
   vazio → PARE e peça curadoria (pesquisar fontes SEM copyright, destilar em `directions/`).
2. NUNCA copie o design distintivo de um produto real. Só princípios/tokens destilados de
   fonte aberta, com a licença anotada. (regra do motor de design; clonar é proibido.)
3. SEMPRE ofereça 2 direções DIFERENTES (não 5 variações da mesma) em 2 LINKS separados.
4. NUNCA decida pelo usuário. Ele escolhe; você grava.
5. A escolha só vira lei ✓ quando gravada em ADR-0010 + `tokens.css`. Sem isso, nada de
   construir tela de produção.

## Procedimento (ordem fixa)

1. CONTEXTO → leia CLAUDE.md, os ADRs e (se houver) a spec/tela alvo. Rode a heurística
   "contexto → direção" da skill `frontend-design`.
2. CATÁLOGO → abra `frontend-design/reference/catalog.md`. Vazio? PARE: diga que o catálogo
   precisa ser curado primeiro (pesquisar fontes SEM copyright) e ofereça fazer a curadoria.
3. PREVIEWS → escolha as 2 direções mais aderentes e gere 2 previews em 2 links, seguindo
   `frontend-design/reference/preview-artboard.md`. Delegue a montagem dos arquivos ao
   `designer` se quiser; a PUBLICAÇÃO dos links é sua (agente principal).
4. ESCOLHA → apresente A e B com motivação + trade-off honesto. PARE e espere o usuário escolher.
5. GRAVAR → com a escolha do usuário:
   - `docs/adr/0010-linguagem-visual.md`: Status Aceito, direção vencedora; a perdedora vira
     "alternativa considerada". Vira lei ✓.
   - `src/styles/tokens.css`: materialize os tokens da direção (ver
     `frontend-design/reference/design-tokens.md`).
6. REPORTAR → resuma a direção escolhida, os arquivos gravados, e o próximo passo: as telas
   agora saem por `/deliver-feature` (o `frontend-engineer` já consome os tokens).

## Depois de gravado
A linguagem visual está fixada. Toda feature de frontend daqui pra frente respeita a
ADR-0010 automaticamente (o `frontend-engineer` lê `tokens.css` + skill `frontend-design`).
Revisar a marca = rodar `/definir-design` de novo (nova direção substitui a ADR-0010).
