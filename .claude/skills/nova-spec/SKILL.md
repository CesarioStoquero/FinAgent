---
name: nova-spec
description: Transforma uma ideia em linguagem natural numa spec bem-formada do FinAgent (docs/specs/<feature>/spec.md), pronta para o usuário validar antes da entrega. Use quando o usuário descrever uma feature/comportamento que quer construir, ex.: "quero permitir saque da carteira", "adiciona extrato". Invocável como /nova-spec.
---

# nova-spec — porta de entrada do FinAgent

Você transforma a intenção do usuário (frase solta) numa `spec.md` que descreve O QUE
e POR QUÊ — nunca o COMO técnico. O usuário VALIDA a spec; só depois o `deliver-feature`
constrói. Você NÃO planeja arquitetura, NÃO escreve código, NÃO cria plan.md/tasks.md.

**Quem executa:** o agente `business-analyst` (papel). Se você é o agente principal, delegue
a ele com um portão de confirmação em vez de escrever a spec você mesmo.

## Invariantes (NUNCA quebre)

1. Spec descreve COMPORTAMENTO observável, não solução técnica. NUNCA cite agregado,
   tabela, endpoint, evento, Kafka — isso é trabalho do `software-engineer`.
2. Toda ambiguidade vira `[NEEDS CLARIFICATION: pergunta]`. NUNCA invente regra de
   negócio (limite de saque, permitir saldo negativo, moeda default) — pergunte.
3. SEMPRE siga a estrutura de `docs/specs/TEMPLATE.md`. Não crie seções novas.
4. Requisitos (RF-x) e critérios de aceite SEMPRE testáveis (verificáveis por teste).
5. Dinheiro sempre em centavos; deixe isso explícito na seção Regras de negócio.
6. SEMPRE preencha a **seção 7 (Restrições herdadas)** citando por ID as RN-x / LGPD-x /
   ADR-x aplicáveis. Regra **✓** é lei → vira RF-x e critério de aceite. Regra **⚠** NUNCA
   entra como lei → vira pendência na seção 10. Sem ID, não é restrição do projeto.

## Procedimento (ordem fixa)

1. LER a intenção do usuário. Se vier vazia, PARE e pergunte qual feature.
2. ROTEAR o módulo em `docs/modules/README.md`. Tocou dado pessoal (nome, CPF, e-mail,
   telefone)? o módulo **compliance** também se aplica — ele é transversal.
3. LER as regras: primeiro o catálogo `docs/business-rules.md` (tem o status ✓/⚠), depois
   o detalhe em `docs/modules/<modulo>/business-rules.md` só dos IDs que se aplicam.
4. ABRIR `docs/specs/TEMPLATE.md` e usar como esqueleto.
5. PREENCHER o que dá para inferir com segurança da intenção + das regras + dos ADRs em
   `docs/adr/`. A seção 7 recebe os IDs; as seções 5 e 11, o que eles exigem.
6. MARCAR como `[NEEDS CLARIFICATION: …]` tudo que for decisão de negócio não dita:
   - limites (valor máximo/mínimo de saque?), saldo negativo permitido?, moeda default?
   - toda regra **⚠** que a feature encostaria (ex.: "adotar RN-7? hoje está ⚠").
   - a feature é operável por linguagem natural (vira tool MCP)? (seção 8)
   - atores/permissões não explicitados.
7. ESCREVER em `docs/specs/<slug-da-feature>/spec.md` (crie a pasta).
8. REPORTAR ao usuário: resumo de 2 linhas + a LISTA de pendências (seção 10) que ele
   precisa decidir. Peça para ele validar ou responder as pendências.

## Regra de parada

Se houver pendências, deixe claro: "não dá para entregar enquanto estas decisões não
forem tomadas". Quando o usuário responder, atualize a spec e mude Status para "Validada".
Quem muda o Status é o usuário, não você.

## Regra nova de negócio

Se o usuário decidir uma regra que ainda não existe no catálogo, avise que ela precisa ser
registrada em `docs/business-rules.md` (+ detalhe no módulo) para virar lei do projeto —
senão ela morre nesta spec.

## Modo lote (backlog → todas as specs de uma vez)

Quando o usuário quiser as specs de VÁRIOS itens (ex.: "gera as specs do backlog"), NÃO faça
uma por conversa. Delegue TUDO numa única chamada ao `business-analyst`, passando:

- a lista item → caminho do arquivo (`BL-x` → `docs/specs/<slug>/spec.md`);
- a instrução de puxar de `docs/backlog.md` o "O quê", "Satisfaz", "Restringido por" e as
  "Decisões abertas" de cada item (as decisões abertas JÁ SÃO pendências da seção 10);
- a exigência de coerência entre as specs do lote: mesmo vocabulário, sem contradição;
- o pedido de um relatório com TODAS as pendências AGRUPADAS (transversais primeiro), para o
  usuário responder de uma vez só.

**Quem NÃO entra no lote:**
- item `Tipo: chore` — não tem spec, vai direto ao `software-engineer`;
- item marcado **DEPENDE DE DECISÃO (⚠)** — a regra ainda não é lei; escrever a spec seria
  transformar sugestão em requisito. Fica de fora até o usuário decidir.

Um lote não pula a validação: as specs nascem **Rascunho** com as pendências abertas, e só o
usuário muda para "Validada".

## Depois da spec validada

Diga ao usuário o próximo passo, literal:
`/deliver-feature docs/specs/<slug>/spec.md`
