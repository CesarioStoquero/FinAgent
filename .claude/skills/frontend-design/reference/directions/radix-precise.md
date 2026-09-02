# Direção: Radix Precise (`radix-precise`)

- **Status:** Curada  ·  **Fonte destilada:** Radix Colors (escalas slate/indigo/jade/red) · **Licença:** MIT (© WorkOS)
- **Fontes tipográficas:** Space Grotesk · Public Sans · IBM Plex Mono — todas **OFL**.
- **Quando usar:** app de dados/fintech claro, tabelas, confiança e clareza (estilo Stripe/Mercury/Linear, sem clonar nenhum).
- **Personalidade em uma frase:** "fintech precisa e sóbria — clara, densa e confiável."

> Destilado de fonte SEM copyright: tokens da escala de 12 passos do Radix (MIT). Valores
> conferidos no repositório oficial; papéis mapeados pelo modelo de escala (ver design-craft/
> color-systems.md). Nunca clona markup — só os valores.

## Tokens (viram `src/styles/tokens.css` se escolhida)

### Cor — tema claro (papel da escala Radix entre parênteses)
| Token | Valor | Uso (passo) |
|-------|-------|-------------|
| `--bg-0`       | `#FCFCFD` | fundo da página (slate 1) |
| `--bg-1`       | `#FFFFFF` | painéis / cards (branco) |
| `--bg-2`       | `#F0F0F3` | preenchimento sutil / hover (slate 3) |
| `--line`       | `#D9D9E0` | separador / borda (slate 6) |
| `--line-strong`| `#CDCED6` | borda de componente (slate 7) |
| `--text-0`     | `#1C2024` | texto primário (slate 12) |
| `--text-1`     | `#60646C` | texto secundário (slate 11) |
| `--accent`     | `#3E63DD` | acento sólido / marca (indigo 9) |
| `--accent-hover` | `#3358D4` | hover do acento (indigo 10) |
| `--accent-text`  | `#3A5BC7` | acento como texto/link (indigo 11) |
| `--pos`        | `#208368` | crédito / positivo (jade 11) |
| `--neg`        | `#CE2C31` | débito / negativo (red 11) |

### Tipografia
| Papel | Família | Fallback |
|-------|---------|----------|
| Display / títulos | Space Grotesk | ui-sans-serif, system-ui, sans-serif |
| Corpo / UI        | Public Sans   | system-ui, sans-serif |
| Números / ids (mono) | IBM Plex Mono | ui-monospace, monospace |

### Forma & ritmo
| Token | Valor |
|-------|-------|
| `--radius` | 6px |
| escala de `--space` | 4 · 8 · 12 · 16 · 24 · 32 · 48 |
| densidade | confortável-densa |

## Vocabulário de componente
Botão primário = preenchimento indigo 9, texto branco, raio 6px, foco com anel indigo 7.
Card = branco sobre fundo slate 1, borda hairline slate 6, sombra quase nula. Tabela densa:
zebra sutil (slate 2), cabeçalho em caixa alta pequena `--text-1`, números à direita em mono
tabular. Input = borda slate 7, foco indigo. Chip/badge = fundo slate 3, texto slate 11.

## O que a torna memorável (anti-genérico)
Neutros levemente frios do slate + um único índigo afiado como acento, tudo alinhado à grade
com números mono batendo coluna. Precisão silenciosa — nada de gradiente ou sombra difusa.
