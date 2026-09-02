---
name: deliver-feature
description: Orquestra a entrega de uma feature de ponta a ponta no FinAgent, encadeando os subagentes COM CONFIRMAÇÃO a cada passo. Aceita uma ideia ("faça tal funcionalidade") ou o caminho de uma spec. Detecta quais agentes a feature precisa e pede sua autorização antes de chamar cada um. Invocável como /deliver-feature.
---

# deliver-feature — orquestrador do FinAgent (com portões de confirmação)

Você (agente principal) orquestra a entrega, delegando aos subagentes. NÃO implemente você
mesmo. A marca deste fluxo: você DETECTA quais agentes a feature precisa e PEDE AUTORIZAÇÃO
antes de chamar cada um — o usuário comanda cada passo.

## Entrada
- Uma IDEIA ("faça o saque da carteira") → você começa criando a spec (passo 0).
- OU o caminho de uma spec já validada (`docs/specs/<x>/spec.md`) → pule para o passo 1.

## Modo de confirmação (o coração deste fluxo)
- **Padrão — passo a passo.** Antes de CADA agente, diga em uma linha QUAL agente, POR QUÊ
  (a detecção) e PERGUNTE "posso chamar?". Só invoque com o "sim". Se "não", pare ou ajuste.
- **Autopilot (opt-in).** Se o usuário disser algo como "pode chamar todos sem perguntar",
  rode sem os portões — mantendo APENAS as paradas obrigatórias (ambiguidade na spec e decisão
  de negócio). Ele volta ao passo a passo quando quiser.

## Invariantes (NUNCA quebre)
1. NUNCA implemente código você mesmo. Delegue ao subagente certo.
2. NUNCA chame um subagente sem AUTORIZAÇÃO do usuário (salvo autopilot explícito).
3. NUNCA avance com ambiguidade. `[NEEDS CLARIFICATION]` → repasse e PARE.
4. UMA tarefa por vez; só avança quando os testes da atual passam.
5. Item "Crítico" do review → volta ao engenheiro, corrige, revisa DE NOVO até zerar.
6. PR só depois de zero críticos e testes verdes.
7. Frontend sem visual definido: se há tarefa `[frontend]` e a ADR-0010 não está "Aceito"
   (nenhuma direção escolhida), o `frontend-engineer` NÃO começa — o `designer` define a
   direção antes (você propõe isso no roteamento).

## Passo 0 — Spec (só se a entrada for uma ideia)
Portão: "Vou chamar o `business-analyst` para escrever a spec de <ideia>. Posso?" Com o "sim",
delegue: "Escreva a spec de <ideia> seguindo o fluxo `nova-spec`. Roteie o módulo, leia as
regras (catálogo + módulo) e preencha as Restrições herdadas por ID."
Mostre o resumo + as pendências `[NEEDS CLARIFICATION]`. PARE e peça o usuário validar/responder.
Sem spec validada, não planeja. (Se a entrada for um item `chore` do backlog, NÃO há spec: pule
direto para o passo 1.)

## Passo 1 — Planejar (portão → software-engineer)
Portão: "Vou chamar o `software-engineer` para planejar <spec>. Posso?" Com o "sim", delegue:
"Planeje a feature em <spec>. Leia os ADRs, resolva/liste pendências, gere plan.md e tasks.md."
- Retornou `[NEEDS CLARIFICATION]`? Repasse e PARE.

## Passo 2 — Rotear (a detecção: quem a feature precisa)
Leia o `tasks.md` e a spec. Monte o PLANO DE AGENTES cruzando os sinais (considere TODOS —
não esqueça nenhum):

| Sinal na spec/tasks | Agente | Por quê |
|---------------------|--------|---------|
| tarefa `[backend]` | `backend-engineer` | implementar (TDD) |
| tarefa `[frontend]` **e** (ADR-0010 não "Aceito" **ou** tela/visual novo) | `designer` (ANTES do frontend) | definir/inovar a direção visual (`/definir-design`) |
| tarefa `[frontend]` **e** ADR-0010 "Aceito" e tela padrão | `frontend-engineer` | implementar contra os tokens |
| toca domínio wallet (saldo, saque, extrato…) | `wallet-specialist` | validar RN-x |
| toca dado pessoal (nome, CPF, e-mail…) | `compliance-specialist` | validar LGPD-x |
| sempre, ao fechar um conjunto de tarefas | `code-reviewer` | revisar contra ADRs/skills |

Apresente o plano: "Detectei que esta feature precisa de: A (por X), B (por Y)… Nesta ordem,
te pedindo autorização antes de cada um. Ok?" Diga também quando NÃO vê necessidade de um agente
(ex.: "visual já definido na ADR-0010, não vejo necessidade do `designer`"). O usuário pode
tirar/adicionar agente — ajuste o plano.

## Passo 3 — Executar (um portão por agente, em ordem)
Para cada agente do plano, EM ORDEM:
1. **Portão:** "Próximo: `<agente>` para <o quê>. Posso chamar?" Só com o "sim".
2. Delegue UMA unidade de trabalho: uma tarefa `[backend]`/`[frontend]`; a direção visual
   (designer); a validação de domínio/LGPD. Design vem ANTES do frontend-engineer quando o
   plano previu o `designer`.
3. Reporte o resultado e vá ao próximo portão. Os testes da tarefa têm que passar antes de seguir.

## Passo 4 — Revisar (portões → specialists + code-reviewer)
Ao fechar um conjunto coeso: portão antes de CADA revisor.
- DOMÍNIO: `wallet-specialist` (RN-x) — violação = Crítico.
- COMPLIANCE (se toca PII): `compliance-specialist` (LGPD-x) — violação = Crítico.
- CÓDIGO: `code-reviewer` (diff contra ADRs/skills).
- "Crítico" (de qualquer um) → volta ao engenheiro (com portão), corrige, revisa de novo.

## Passo 5 — PR (portão → gh)
Portão: "Zero críticos e testes verdes. Posso abrir o PR?" Com o "sim", siga
`reference/pull-request.md`. Se o `gh` não estiver instalado/autenticado, pule e avise.

## Passo 6 — Reportar
Resuma: o que foi entregue, quais agentes foram chamados (e quais você pulou e por quê),
build/testes, parecer do revisor, link do PR (se aberto).

## Economia de token (não over-orquestre)
Cada subagente é um contexto novo = tokens. O roteamento existe pra chamar SÓ o necessário:
specialist de domínio só quando toca o domínio; compliance só com PII; `designer` só quando há
visual novo/indefinido. Reserve Opus para decisão real (planejamento, design); o mecânico roda
em modelo menor.

## Diante de decisão de negócio
PARE e pergunte. ADRs vencem preferências pessoais. Nunca decida regra de negócio sozinho.
