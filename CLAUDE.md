# FinAgent — instruções do projeto (constituição)

Backend de carteira/ledger financeiro em microsserviços, operável por linguagem
natural via camada agêntica (MCP + agente). Sistema AI-first.

## Princípios (derivados dos ADRs em docs/adr/)
- Arquitetura Hexagonal: dependências apontam pra dentro; regra no Domain (ADR-0003).
- Event Sourcing no ledger: estado derivado de eventos imutáveis (ADR-0001).
- CQRS: escrita via agregado; leitura via read model (ADR-0002).
- Kafka entre serviços (ADR-0004); PostgreSQL como event store (ADR-0007);
  MongoDB no read model (ADR-0005); camada agêntica via MCP (ADR-0006).
- Dinheiro sempre em centavos (long). Proibido float em valor monetário.
- `TreatWarningsAsErrors` ligado; TDD como padrão.

## Como trabalhar aqui (o harness)
Este projeto usa um time de subagentes + skills em `.claude/`:
- Skills (`.claude/skills/`): padrões de backend .NET e frontend Angular. O Claude
  carrega automaticamente conforme a tarefa.
- Agentes (`.claude/agents/`): `software-engineer` (planeja), `backend-engineer` e
  `frontend-engineer` (implementam consumindo as skills), `code-reviewer` (revisa).
- Orquestrador: a skill `deliver-feature` (`/deliver-feature`) encadeia tudo.

### Uso típico
- Planejar + implementar uma spec de uma vez:
  `/deliver-feature docs/specs/wallet/spec.md`
- Ou passo a passo:
  "Use o software-engineer para planejar docs/specs/wallet/spec.md"
  depois "Use o backend-engineer para implementar as tarefas [backend] do tasks.md"
  depois "Use o code-reviewer para revisar as mudanças".

## Integração com Spec Kit
As specs em `docs/specs/` seguem o Spec Kit. A constituição do Spec Kit deve apontar
para os ADRs. O `software-engineer` pode usar ou complementar `/speckit.plan` e
`/speckit.tasks`; o `deliver-feature` assume que existe uma spec já escrita.
