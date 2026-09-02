# Direção: Editorial Sand (`editorial-sand`)

- **Status:** Curada  ·  **Fonte destilada:** Radix Colors (escalas sand/amber/jade/red) · **Licença:** MIT (© WorkOS)
- **Fontes tipográficas:** Newsreader · Source Sans 3 · IBM Plex Mono — todas **OFL**.
- **Quando usar:** marca/onboarding/landing e telas de primeira impressão; quando "chamar atenção" e ter voz autoral pesam mais que densidade.
- **Personalidade em uma frase:** "editorial caloroso — tipografia grande, contraste serif+grotesk, autoral."

> Destilado de fonte SEM copyright: neutros quentes da escala sand + acento bronze do amber
> (Radix, MIT). Só valores; a personalidade vem do par tipográfico e da composição.

## Tokens (viram `src/styles/tokens.css` se escolhida)

### Cor — tema claro quente (passo Radix entre parênteses)
| Token | Valor | Uso |
|-------|-------|-----|
| `--bg-0`        | `#FDFDFC` | fundo (sand 1) |
| `--bg-1`        | `#FFFFFF` | painéis / cards |
| `--bg-2`        | `#F1F0EF` | preenchimento sutil (sand 3) |
| `--line`        | `#DAD9D6` | separador / borda (sand 6) |
| `--text-0`      | `#21201C` | texto primário (sand 12) |
| `--text-1`      | `#63635E` | texto secundário (sand 11) |
| `--accent`      | `#AB6400` | acento como texto/marca — bronze (amber 11) |
| `--accent-solid`| `#FFC53D` | acento sólido / destaque (amber 9) |
| `--pos`         | `#208368` | crédito / positivo (jade 11) |
| `--neg`         | `#CE2C31` | débito / negativo (red 11) |

### Tipografia
| Papel | Família | Fallback |
|-------|---------|----------|
| Display / títulos (serif) | Newsreader | Georgia, "Times New Roman", serif |
| Corpo / UI (sans)         | Source Sans 3 | system-ui, sans-serif |
| Números / ids (mono)      | IBM Plex Mono | ui-monospace, monospace |

### Forma & ritmo
| Token | Valor |
|-------|-------|
| `--radius` | 8px |
| escala de `--space` | 4 · 8 · 12 · 16 · 24 · 40 · 64 (mais respiro) |
| densidade | arejada |

## Vocabulário de componente
Título = serif Newsreader GRANDE (display), contrastando com corpo grotesco Source Sans.
Acento bronze reservado a UMA chamada por seção. Card = branco, borda sand 6, bastante ar
interno. Botão primário = bronze sólido ou contorno; foco com anel âmbar. Assimetria proposital
no layout (não tudo centralizado). Números ainda em mono tabular quando forem dados.

## O que a torna memorável (anti-genérico)
O contraste serif de display + grotesco de corpo, neutros quentes e um bronze escasso. Voz de
revista, não de template SaaS — foge por completo do "Inter + card arredondado".
