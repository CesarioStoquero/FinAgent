# Direção: Carbon Dark (`carbon-dark`)

- **Status:** Curada  ·  **Fonte destilada:** IBM Carbon Design System — tema Gray 100 (dark) · **Licença:** Apache-2.0
- **Fontes tipográficas:** IBM Plex Sans · IBM Plex Mono — **OFL**.
- **Quando usar:** produto AI-first / técnico, ferramenta de engenharia, dados densos no escuro (combina com MCP + event sourcing).
- **Personalidade em uma frase:** "instrumento de engenharia financeira — escuro, técnico e afiado."

> Destilado de fonte SEM copyright: tokens do tema g100 do Carbon (Apache-2.0), mapeados da
> paleta base IBM (@carbon/colors). Só valores; nunca o markup dos componentes Carbon.

## Tokens (viram `src/styles/tokens.css` se escolhida)

### Cor — tema escuro (token Carbon g100 / passo da paleta)
| Token | Valor | Uso |
|-------|-------|-----|
| `--bg-0`        | `#161616` | fundo (background · gray 100) |
| `--bg-1`        | `#262626` | painéis (layer-01 · gray 90) |
| `--bg-2`        | `#393939` | camada 2 / hover (layer-02 · gray 80) |
| `--line`        | `#393939` | borda sutil (border-subtle) |
| `--line-strong` | `#525252` | borda forte (gray 70) |
| `--text-0`      | `#F4F4F4` | texto primário (gray 10) |
| `--text-1`      | `#C6C6C6` | texto secundário (gray 30) |
| `--text-2`      | `#8D8D8D` | helper / placeholder (gray 50) |
| `--accent`      | `#4589FF` | interativo / marca (blue 50) |
| `--accent-text` | `#78A9FF` | link (blue 40) |
| `--pos`         | `#42BE65` | crédito / sucesso (green 40) |
| `--neg`         | `#FA4D56` | débito / erro (red 50) |
| `--warn`        | `#F1C21B` | atenção (yellow 30) |

### Tipografia
| Papel | Família | Fallback |
|-------|---------|----------|
| Display / títulos | IBM Plex Sans | ui-sans-serif, system-ui, sans-serif |
| Corpo / UI        | IBM Plex Sans | system-ui, sans-serif |
| Números / ids (mono) | IBM Plex Mono | ui-monospace, monospace |

### Forma & ritmo
| Token | Valor |
|-------|-------|
| `--radius` | 2px (quase reto — assinatura Carbon) |
| escala de `--space` | 4 · 8 · 12 · 16 · 24 · 32 · 48 |
| densidade | compacta |

## Vocabulário de componente
Botão = retângulo (raio 2px), primário azul 50, foco com anel branco. Superfícies sobem de luz
por CAMADA (bg-0 → bg-1 → bg-2), não por sombra. Tabela = linhas de dados densas, divisórias
`--line`, cabeçalho gray 50 em caixa alta pequena. Número/id/hash sempre em Plex Mono tabular.
Status/pill = borda 1px + texto, sem preenchimento pesado.

## O que a torna memorável (anti-genérico)
Cantos RETOS, fundo em camadas de cinza-quente e um azul elétrico contido; números e ids em
mono por toda parte. Cara de terminal financeiro/instrumento — o oposto do card arredondado de IA.
