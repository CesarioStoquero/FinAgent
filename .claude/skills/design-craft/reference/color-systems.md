# Sistemas de cor

Cor é semântica e hierarquia — não decoração. Trabalhe em ESCALAS e PAPÉIS, nunca hex solto.

## Papéis (o modelo de escala de 12 passos — Radix/Carbon)
Uma cor vira uma escala; cada passo tem um papel fixo. Referência (escala de 12):
- **1-2:** fundo do app / fundo sutil.
- **3-5:** fundo de componente (normal / hover / ativo).
- **6-8:** bordas (separador sutil / borda de componente / borda forte, hover).
- **9-10:** preenchimento SÓLIDO (acento) e seu hover. (o 9 é a cor "de marca".)
- **11:** texto de baixo contraste (secundário) — legível sobre o fundo.
- **12:** texto de alto contraste (primário).
Pensar por papel é o que torna dark/light e a troca de acento triviais.

## Tokens semânticos (nunca o valor cru no componente)
Nomeie pelo PAPEL, não pela cor: `--bg-0`, `--line`, `--text-1`, `--accent`, `--pos`, `--neg`.
`--pos: verde` está errado; `--pos` aponta para o verde da escala. Trocar a marca = trocar o
alvo do token, não caçar hex em 100 telas.

## oklch (por que preferir)
`oklch(L C H)` é perceptualmente uniforme: mesmo L = mesmo brilho aparente entre hues (ao
contrário de HSL). Regra dos acentos: **mesmo C e L, varie só o H** — ficam harmônicos e com
peso igual. Ex.: `--pos: oklch(0.72 0.13 155)` e `--neg: oklch(0.72 0.13 25)`.

## Proporção e contraste
- 60-30-10 (ver `visual-hierarchy.md`): acento é ~10%. Escasso.
- Contraste é lei (AA): texto normal ≥ 4.5:1, grande ≥ 3:1, componente/gráfico ≥ 3:1. Valide
  cada par texto/fundo (ver `accessibility-wcag.md`). Verde/vermelho de valor: garanta que
  passam sozinhos, sem depender só da cor (heurística: nunca cor como único sinal).

## Dark mode
Não é "inverter". Fundo escuro levemente tonalizado (não #000), superfícies sobem de luz por
CAMADA (layer-01/02/03), texto não é branco puro (#f4f4f4). Redefina o MÍNIMO de tokens no
tema; papéis continuam os mesmos.
