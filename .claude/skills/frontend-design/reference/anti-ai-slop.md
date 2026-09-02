# As marcas de "cara de IA" — o que BANIR (checklist)

Destilado do consenso de engenheiros de frontend em 2026: o que faz um site gerado por IA
parecer idêntico a todos os outros. O `frontend-engineer` roda isto antes de concluir a tela.

## Banir (o que denuncia "gerado por IA")

1. **Fonte clichê no corpo:** Inter, Roboto, Arial, Open Sans (e o "Fraunces em tudo"). Use
   um par com personalidade + fallback de métricas próximas (definido na direção).
2. **Gradiente-blob de fundo:** o degradê roxo/azul difuso atrás do hero. Prefira fundo
   chapado com hairlines, textura sutil (grão/ruído leve) ou nada.
3. **Card "genérico de IA":** canto muito arredondado + borda-accent colorida à esquerda +
   sombra difusa grande. Prefira raio contido, borda hairline de 1px, sombra sutil ou nenhuma.
4. **Emoji como ícone.** Sempre SVG inline (stroke, um estilo só).
5. **Número em fonte proporcional.** Dinheiro/quantidade/id em MONO + `tabular-nums`.
6. **Tudo centralizado, mesma medida, mesmo respiro.** Use hierarquia real: uma coisa
   dominante, o resto claramente subordinado; assimetria proposital.
7. **Cor tímida e uniforme.** Uma direção tem cor dominante + acento afiado, não 5 pastéis.
8. **"Data slop":** número/stat/ícone que não serve a nada, só pra encher. Cada elemento
   ganha o lugar. Menos é mais.
9. **Copy genérica:** "Welcome to our platform", "Empowering your finances". Escreva copy
   específica do FinAgent (carteira, extrato, agente, centavos, Pix, LGPD).
10. **Espaçamento por margin/whitespace solto.** Use flex/grid + `gap` (sobrevive à edição e
    fica consistente).

## Em vez disso (o que dá cara de produto real)

- Densidade de "instrumento": tabelas alinhadas, números que batem coluna, hairlines.
- Detalhe técnico honesto do domínio (id de evento, versão do agregado, lag da projeção) —
  quando serve à leitura, não como enfeite.
- Um único traço de assinatura por tela, coerente com a direção do ADR-0010.
