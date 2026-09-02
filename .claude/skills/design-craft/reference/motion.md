# Movimento — com propósito, nunca enfeite

Animação boa é invisível: orienta, dá feedback e mantém continuidade. Animação ruim é decoração
que atrasa o usuário (e grita "template").

## Os 3 usos legítimos
1. **Orientação:** de onde veio / para onde foi (um painel desliza da direita → ele "está" à
   direita). Preserva o modelo espacial.
2. **Feedback:** confirma que a ação registrou (botão reage, item marca, linha entra na lista).
3. **Continuidade:** transição entre estados sem "corte seco" que desorienta.

## Parâmetros
- **Duração:** 150-250ms para a maioria (micro-interação); 250-400ms para transição de tela.
  Rápido demais some; lento demais irrita no uso repetido.
- **Easing:** entra com `ease-out` (rápido→lento), sai com `ease-in`. Nada linear (parece robô)
  exceto para movimento contínuo (spinner).
- **Escassez:** UMA revelação bem orquestrada > vinte micro-animações competindo. No app de uso
  diário, menos movimento; na landing, pode mais.

## Acessibilidade e performance
- SEMPRE respeite `@media (prefers-reduced-motion: reduce)`: reduza/remova. Não é opcional.
- Anime só `transform` e `opacity` (compostas na GPU). Evite animar `width`/`top`/`left`
  (relayout, engasga). Nada que pisque > 3x/s.
