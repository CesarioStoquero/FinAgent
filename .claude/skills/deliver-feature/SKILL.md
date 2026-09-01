---
name: deliver-feature
description: Orquestra a entrega de uma feature de ponta a ponta no FinAgent, encadeando os subagentes. Use quando quiser planejar e implementar uma spec com um único comando, ex.: "entregue a feature de docs/specs/wallet/spec.md". Invocável como /deliver-feature.
---

# deliver-feature — orquestrador do FinAgent

Você (agente principal) orquestra a entrega a partir de uma spec, delegando aos
subagentes. NÃO implemente você mesmo — delegue sempre.

## Entrada

Caminho de uma spec (ex.: `docs/specs/wallet/spec.md`). Se não vier, PARE e pergunte.

## Invariantes (NUNCA quebre)

1. NUNCA implemente código você mesmo. Delegue ao subagente certo.
2. NUNCA avance com ambiguidade. `[NEEDS CLARIFICATION]` → repasse ao usuário e PARE.
3. UMA tarefa por vez. Nada de "big bang". Só avança quando os testes da atual passam.
4. NUNCA invente escopo além do `tasks.md`.
5. Item "Crítico" do review → volta ao engenheiro, corrige, revisa DE NOVO. Repita
   até zerar os críticos.
6. NUNCA abra o PR com review em aberto. PR só depois de zero críticos e testes verdes.

## Procedimento (ordem fixa)

1. PLANEJAR → delegue ao `software-engineer`:
   "Planeje a feature em <spec>. Leia os ADRs, resolva ou liste pendências, gere
   plan.md e tasks.md ao lado da spec."
   - Retornou `[NEEDS CLARIFICATION]`? Repasse ao usuário e PARE. Não prossiga.

2. IMPLEMENTAR → leia o `tasks.md` gerado. Para cada tarefa, EM ORDEM:
   - `[backend]` → delegue ao `backend-engineer` (uma tarefa, com TDD).
   - `[frontend]` → delegue ao `frontend-engineer` (uma tarefa, com testes).
   - Só passe para a próxima quando os testes da atual passarem.

3. REVISAR → ao fechar um conjunto coeso de tarefas, faça DUAS revisões:
   - DOMÍNIO: delegue ao especialista do módulo (ex.: `wallet-specialist`) para validar
     as regras de negócio (RN-x). Violação de RN é tratada como "Crítico".
   - COMPLIANCE (se a feature toca dado pessoal): delegue ao `compliance-specialist`
     (regras LGPD-x). Violação de LGPD é "Crítico".
   - CÓDIGO: delegue ao `code-reviewer` (diff contra ADRs e skills).
   - "Crítico" (de qualquer uma) → volta ao engenheiro; corrige; revisa de novo (invariante 5).

4. ABRIR PR → só com zero críticos e testes verdes (invariante 6). Abra o pull request
   no GitHub via `gh` (GitHub CLI). Siga `reference/pull-request.md` (branch, título,
   descrição). Se o `gh` não estiver instalado/autenticado, PULE este passo e avise o usuário.

5. REPORTAR → resuma ao usuário: o que foi entregue, tarefas concluídas x pendentes,
   resultado de build/testes, parecer do revisor e o link do PR (se aberto).

## Economia de token (não over-orquestre)

Cada subagente acionado é um CONTEXTO NOVO do zero = tokens. Dose o fan-out:
- Feature pequena (1-2 tarefas, sem PII): UMA passada de revisão no `code-reviewer` já basta;
  não acione os especialistas de domínio à toa.
- Especialista de domínio (`wallet-specialist`) só quando a feature mexe naquele domínio;
  `compliance-specialist` só quando toca dado pessoal. Reserve Opus para decisão real
  (planejamento); o mecânico roda em modelo menor.

## Diante de decisão de negócio

PARE e pergunte. ADRs vencem preferências pessoais. Nunca decida regra de negócio sozinho.
