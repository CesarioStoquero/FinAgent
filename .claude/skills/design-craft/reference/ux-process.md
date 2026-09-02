# Processo: da tarefa ao pixel (não pule etapas)

Sênior desenha DEPOIS de entender. A pele (cor/tipo) é a última etapa, não a primeira.

## 1. Entender (antes de qualquer tela)
- **Job To Be Done:** "quando <situação>, quero <motivação>, para <resultado>." Foca na
  tarefa, não na feature. Ex.: "quando recebo um Pix, quero confirmar que caiu, para confiar."
- **Atores e contexto:** quem, com que frequência, em que dispositivo, sob que pressão.
- **Sucesso mensurável:** o que muda se der certo (tempo da tarefa, erro, confiança).

## 2. Arquitetura de informação (IA)
- Agrupe por MODELO MENTAL do usuário, não pelo organograma do backend.
- Nomeie com a palavra do usuário (heurística 2). Teste rótulos com card sorting quando houver dúvida.
- Navegação = as 3-7 áreas de topo. Se passou disso, o modelo está errado, não o menu.

## 3. Fluxo (user flow) antes do layout
- Desenhe o caminho: telas/decisões/estados como um grafo. Marque onde entra erro, vazio,
  espera. Um fluxo bom tem o mínimo de passos e nenhum beco sem saída.

## 4. Escada de fidelidade (não comece bonito)
- **Wireframe low-fi** (cinza, sem cor): resolve estrutura e hierarquia. Explore 3-5 opções.
- **Protótipo:** valida fluxo e interação.
- **Hi-fi:** só quando estrutura e fluxo estão certos — aí aplica a direção visual (tokens).
- Regra: nunca resolva no hi-fi um problema que era de estrutura. Volte um degrau.

## 5. Validar
- Heurísticas (ux-heuristics.md) sempre. Teste com 5 usuários pega ~85% dos problemas quando
  possível. Itere. Decisão de design é hipótese até ser observada.
