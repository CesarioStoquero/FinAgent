# Catálogo de direções visuais — FinAgent

Índice (o "nó de navegação") das direções de design que o `/definir-design` pode oferecer.
Cada linha aponta para um arquivo `directions/<id>.md` com os tokens destilados. Todas
destiladas de fontes SEM copyright (MIT/Apache-2.0/OFL) — princípios e valores, nunca markup.

## Regras da curadoria (ao adicionar uma direção)

- Destilar PRINCÍPIOS e TOKENS de fontes abertas — NUNCA copiar o markup nem o "jeitão"
  distintivo de um produto real (é violação; proibido).
- Registrar a fonte e a LICENÇA de cada direção (cor e tipografia).
- Uma direção = uma personalidade coerente (paleta, par tipográfico, densidade, raios,
  vocabulário de componente), preenchida a partir de `directions/_TEMPLATE.md`.
- Manter poucas direções fortes e distintas — não 10 variações do mesmo tema.

## Índice

| Direção (id) | Quando usar | Arquivo | Fonte destilada | Licença |
|--------------|-------------|---------|-----------------|---------|
| `radix-precise` | fintech clara, dados densos, confiança | `directions/radix-precise.md` | Radix Colors | MIT |
| `carbon-dark` | AI-first / técnico, escuro, instrumento | `directions/carbon-dark.md` | IBM Carbon (g100) | Apache-2.0 |
| `editorial-sand` | marca / onboarding / landing, autoral | `directions/editorial-sand.md` | Radix Colors (sand/amber) | MIT |

> Tipografia de todas: fontes OFL (Space Grotesk, Public Sans, IBM Plex Sans/Mono, Newsreader,
> Source Sans 3). Ampliar o catálogo: clone `directions/_TEMPLATE.md`, destile de outra fonte
> aberta (ex.: GitHub Primer — MIT, Open Props — MIT, Material 3 — Apache-2.0) e some a linha acima.

## Fontes abertas candidatas (para expandir depois)
- **Open Props** (MIT) — arquivo de CSS variables (cor/spacing/sombra); o mais fácil de destilar.
- **GitHub Primer / Primer Primitives** (MIT) — design system dev-tool.
- **Material Design 3 / Angular Material** (Apache-2.0 / MIT) — tokens + lib Angular real.
- **PrimeNG / PrimeUIX** (MIT) — temas como tokens + lib Angular real.
