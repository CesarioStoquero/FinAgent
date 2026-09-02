# Acessibilidade — WCAG 2.2 AA (concreto)

Padrão de fato (W3C, recomendação desde 2023; base de leis como EAA/ADA). AA é o alvo. Não é
fase 2 — é definição de "pronto".

## Contraste (mínimos AA)
- Texto normal: **≥ 4.5:1** contra o fundo.
- Texto grande (≥ 24px, ou ≥ 18.66px/14pt bold): **≥ 3:1**.
- Componentes de UI e gráficos essenciais (borda de input, ícone que informa, série de
  gráfico): **≥ 3:1**.
- Valide cada par (ferramenta de contraste). Cor nunca é o ÚNICO sinal (adicione ícone/rótulo).

## Novos critérios da 2.2 que afetam design
- **2.5.8 Target Size (mín):** alvo interativo **≥ 24×24 px** CSS (com padding), com espaço
  entre alvos. Conforto de toque real: mire **44px** (guia de mobile).
- **2.4.11 Foco não obscurecido:** ao focar via teclado, o componente não fica escondido atrás
  de header fixo/sticky. Reserve o offset.
- **2.5.7 Dragging:** toda ação de arrastar tem alternativa por clique (ex.: botões além do
  slider).
- **3.2.6 Ajuda consistente:** se há ajuda, no mesmo lugar relativo em todas as telas.
- **3.3.8 Autenticação acessível:** não exija decorar/transcrever; permita colar e gerenciador
  de senha.

## Sempre
- **Teclado:** tudo operável sem mouse; ordem de foco lógica; **foco visível** (anel nítido,
  não só `outline:none`).
- **Semântica:** HTML correto (`button`, `label` ligado ao campo, cabeçalhos em ordem, `nav`,
  `main`). ARIA só quando o HTML nativo não dá conta.
- **Formulário:** rótulo visível, erro descritivo ligado ao campo, instrução antes do erro.
- **Status messages (4.1.3):** mudanças dinâmicas (salvo, erro, item adicionado) anunciadas a
  leitor de tela (`aria-live`).
- **Movimento:** respeite `prefers-reduced-motion`; nada que pisque > 3x/s.
- **Zoom:** usável a 200% e em 320px de largura sem scroll horizontal (reflow).
