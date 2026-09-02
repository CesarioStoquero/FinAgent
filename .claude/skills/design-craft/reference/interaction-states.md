# Estados — o que separa protótipo de produto

O "caminho feliz" é ~30% do design. O sênior modela TODOS os estados; o júnior esquece e o
app quebra na vida real.

## Estados de COMPONENTE (todo controle)
`default · hover · focus (visível!) · active/pressed · disabled · loading · error · selected`
- Cada um distinto e acessível (foco nunca só por cor). Disabled tem contraste suficiente para
  ser lido, mas claramente inativo.
- Botão que dispara ação assíncrona: vai para `loading` (spinner + rótulo) e trava re-clique.

## Estados de TELA (toda página/lista/card)
1. **Vazio (empty):** primeira vez / sem dados. NÃO é uma tela em branco — é onboarding: diga
   o que é, por que está vazio, e a ação para preencher. (a mais esquecida.)
2. **Carregando:** skeleton com a forma do conteúdo > spinner solto. Preserva o layout, reduz
   a sensação de espera. Para ação rápida, use UI otimista (mostra o resultado e reconcilia).
3. **Parcial / carregando mais:** paginação/scroll com indicador; erro de página não derruba o
   resto.
4. **Erro:** o que houve, em linguagem humana, + a saída (tentar de novo, contato). Nunca só
   "erro 500". Preserve o que o usuário digitou.
5. **Ideal (denso):** teste com MUITO dado real (nome longo, valor grande, lista cheia) — é
   onde o layout quebra. Desenhe para o caso cheio, não só o vazio bonito.

## Mensagem de erro (padrão)
`[o que aconteceu] + [por quê, se útil] + [o que fazer agora]`. Ex.: "Não deu para carregar o
extrato (sem conexão). Tentar de novo." — não "Falha na requisição".

## Feedback (heurística 1)
Toda ação → confirmação visível ≤ ~1s. Sucesso discreto (toast/checkmark), erro claro e
persistente até resolver.
