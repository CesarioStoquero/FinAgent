# As 10 heurísticas de usabilidade de Nielsen (avaliar uma tela)

O critério clássico e ainda padrão (Nielsen Norman Group) para achar problema de usabilidade
sem usuário na sala. Rode como checklist ao revisar qualquer fluxo/tela.

1. **Visibilidade do status do sistema.** O sistema sempre diz o que está acontecendo
   (carregando, salvo, enviado, saldo atualizado). *Check:* toda ação tem feedback ≤ ~1s?
2. **Correspondência com o mundo real.** Linguagem do usuário, não do sistema. *Check:* usei
   "extrato", "saldo", "Pix" — não "read model", "aggregate", "event"?
3. **Controle e liberdade.** Saída clara: desfazer, cancelar, voltar. *Check:* toda ação
   arriscada tem escape/undo? Nada de becos sem saída.
4. **Consistência e padrões.** Mesma coisa, mesmo nome, mesmo lugar (interno e do mercado).
   *Check:* botão primário, datas, moeda, ícones — idênticos em todo o app?
5. **Prevenção de erro.** Melhor evitar que tratar. *Check:* desabilitei o que não pode?
   confirmei o irreversível? validei antes de submeter?
6. **Reconhecer em vez de lembrar.** Opções e dados visíveis; não faça decorar. *Check:* o
   usuário vê o que precisa ou tem que lembrar de outra tela?
7. **Flexibilidade e eficiência.** Atalhos para experientes sem atrapalhar o novato.
   *Check:* tem acelerador (⌘K, filtros salvos) sem poluir o caminho básico?
8. **Estética e design minimalista.** Nada compete com o essencial. *Check:* removi todo
   elemento que não serve à tarefa? (o "data slop").
9. **Ajudar a reconhecer, diagnosticar e recuperar de erros.** Mensagem em linguagem humana:
   o que houve + como resolver. *Check:* erro diz a saída, não só "algo deu errado"?
10. **Ajuda e documentação.** Quando precisar, ajuda achável e no contexto. *Check:* dá para
    achar ajuda sem sair da tarefa (heurística 3.2.6 do WCAG: ajuda consistente)?

## Como usar
Passe as 10 numa tela e anote violação + severidade (crítico/aviso/sugestão), como o
`code-reviewer` faz com código. Heurística é achado, não gosto — cite a de número N.
