# Gerar `src/styles/tokens.css` a partir da direção escolhida

Quando o usuário escolhe uma direção no `/definir-design`, materialize os tokens dela (de
`directions/<id>.md`) neste arquivo. É a FONTE ÚNICA que todo componente consome.

## Local e forma
- Arquivo: `src/styles/tokens.css` (importado no `styles.css`/`styles.scss` global do app).
- Tudo em CSS custom properties no `:root`. Componentes usam `var(--token)` — nunca o valor.
- Tema (se a direção for dark/light): `:root` = base; sobrescreva o MÍNIMO em
  `@media (prefers-color-scheme: ...)` e/ou `:root[data-theme="..."]`.

## Esqueleto (preencha com os valores da direção — `[A DEFINIR]` até a escolha)

```css
:root {
  /* Cor — valores oklch da direção escolhida (ADR-0010) */
  --bg-0: /* [A DEFINIR] */;   /* fundo da página  */
  --bg-1: /* [A DEFINIR] */;   /* painéis / cards  */
  --line: /* [A DEFINIR] */;   /* bordas / hairline */
  --text-0: /* [A DEFINIR] */; /* texto primário   */
  --text-1: /* [A DEFINIR] */; /* texto secundário */
  --accent: /* [A DEFINIR] */; /* marca / interativo */
  --pos: /* [A DEFINIR] */;    /* crédito (positivo) */
  --neg: /* [A DEFINIR] */;    /* débito  (negativo) */

  /* Tipografia */
  --font-display: /* [A DEFINIR] */, system-ui, sans-serif;
  --font-sans:    /* [A DEFINIR] */, system-ui, sans-serif;
  --font-mono:    /* [A DEFINIR] */, ui-monospace, monospace;

  /* Forma & ritmo */
  --radius: /* [A DEFINIR] */;
  --space-1: 4px; --space-2: 8px; --space-3: 12px; --space-4: 16px;
  --space-6: 24px; --space-8: 32px; /* base 4/8; a direção pode ajustar */
}
```

## Regra de ouro
Token novo entra AQUI primeiro, com nome SEMÂNTICO (`--pos`, não `--verde`). O componente só
referencia. É isso que mantém 100 telas coerentes e a troca de direção barata.
