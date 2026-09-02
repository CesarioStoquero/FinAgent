# Tipografia

O maior levantador de "cara de produto real" — ou de "cara de IA", quando é a fonte clichê.

## Escolha (1-3 fontes, com personalidade + fallback de métricas próximas)
- **Evite no corpo:** Inter, Roboto, Arial, Open Sans (e "Fraunces em tudo"). São o default
  de 90% dos sites gerados por IA.
- **Par típico:** um display característico (títulos) + um corpo legível + um mono para
  números/ids. Ex. de fontes abertas (OFL): IBM Plex Sans/Mono, Space Grotesk, Source Serif 4,
  Newsreader, Source Sans 3, Geist, JetBrains Mono.
- **Fallback:** sempre uma pilha (`'IBM Plex Sans', system-ui, sans-serif`) com métricas
  próximas — a fonte web pode não carregar (e o export de PDF usa o fallback).

## Escala (modular, não aleatória)
- Uma razão só: 1.2 (denso/ferramenta) ou 1.25 (arejado). Degraus ex.: 12 · 14 · 16 · 20 · 25 ·
  31 · 39. Corpo base 14-16px. Cada nível visivelmente diferente do vizinho.

## Legibilidade
- **Medida:** 45-75 caracteres por linha no corpo. Coluna larga demais cansa.
- **Entrelinha:** ~1.5 no corpo, mais apertada (1.1-1.25) em títulos grandes.
- **Peso:** 2-3 pesos bastam (ex. 400/500/700). Não use 8 pesos.
- **Números:** SEMPRE `font-variant-numeric: tabular-nums` em dinheiro/tabela/id, em fonte
  mono — as colunas batem e some a "cara de IA" do número proporcional.
- `text-wrap: pretty`/`balance` em títulos evita viúvas.

## Hierarquia por tipo
Título domina por TAMANHO + peso; rótulos em caixa alta pequena com `letter-spacing` leve;
corpo neutro; legenda menor e em `--text-1`. Menos estilos, aplicados com consistência.
