# Passo canônico — Abrir o PR no GitHub (gh CLI)

Executado pelo orquestrador APÓS zero críticos no review e testes verdes.
Usa o **GitHub CLI (`gh`)** — nativo do Claude Code, sem MCP. Se o `gh` não estiver
instalado/autenticado, pule e avise o usuário.

## Fluxo

1. Garanta que as mudanças estão numa branch de feature (não em `main`):
   - Branch: `feature/<slug-da-spec>` (ex.: `feature/saque-carteira`).
   - `git add -A` e commit com mensagem no imperativo.
2. Publique a branch: `git push -u origin feature/<slug>`.
3. Abra o PR com o template de descrição abaixo:
   ```
   gh pr create --base main --head feature/<slug> --title "<tipo>: <resumo>" --body-file <arquivo>
   ```
   - `<tipo>`: feat / fix / refactor (ex.: `feat: saque de carteira`).
   - Prefira `--body-file` apontando para um arquivo temporário com a descrição (evita
     problemas de escape de aspas/acentos no PowerShell).
4. O `gh` imprime a **URL do PR** — devolva ao usuário.

## Template da descrição do PR

```markdown
## Contexto
<objetivo da spec, seção 1 — por que esta mudança existe>

Spec: `docs/specs/<slug>/spec.md`

## O que mudou
<lista das tarefas concluídas do tasks.md, uma linha cada>

## Como testar
<passos/endpoints ou o comando de teste; o que o revisor roda para ver funcionando>

## Checklist
- [x] Testes de unidade e integração passando
- [x] Build limpo com warnings-as-errors
- [x] Revisado pelo code-reviewer, zero itens críticos
- [x] Respeita os ADRs (hexagonal, event sourcing, partida dobrada, dinheiro em centavos)
```

## Erros a evitar

- Abrir PR de `main` para `main` — sempre de uma branch de feature.
- Descrição genérica ("várias mudanças"). Puxe o conteúdo real da spec e do tasks.md.
- Abrir PR com review em aberto ou teste vermelho (invariante 6 do SKILL.md).
- Escapar aspas/acentos na `--body` inline no PowerShell — use `--body-file`.
