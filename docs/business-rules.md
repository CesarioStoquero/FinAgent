# Catálogo de Regras de Negócio — FinAgent

**Fonte da verdade central.** Toda regra de negócio do FinAgent está listada aqui, por ID.
Este é o único lugar para ver TODAS as regras de uma vez; o detalhe fica no arquivo do
módulo. Specs, testes, código e agentes referenciam pelo ID.

**Origem (para nada genérico invalidar o projeto):**
- **✓ Confirmada** — já é decisão do FinAgent, consta em ADR ou CLAUDE.md.
- **⚠ Confirmar** — princípio importado de fora; sólido, mas você precisa bater o martelo
  antes de virar lei. Enquanto for ⚠, o agente trata como sugestão, não como regra dura.

## Wallet / Ledger

| ID | Regra (enunciado do FinAgent) | Origem | Fonte |
|----|-------------------------------|--------|-------|
| RN-2 | Saldo é derivado dos eventos; nunca coluna mutável fonte da verdade. | ✓ Confirmada | ADR-0001 |
| RN-3 | Evento postado é imutável: estado vem do replay, sem UPDATE/DELETE. | ✓ Confirmada | ADR-0001 |
| RN-5 | Nenhum movimento de dinheiro sem um evento correspondente. | ✓ Confirmada | ADR-0001 |
| RN-6 | Todo centavo do saldo é explicável percorrendo os eventos, sem lacunas. | ✓ Confirmada | ADR-0001 |
| RN-8 | Todo valor é inteiro de centavos (long); proibido float/fração de centavo. | ✓ Confirmada | CLAUDE.md |
| RN-9 | Operar entre moedas diferentes lança exceção (sem soma direta). | ✓ Confirmada | ADR-0001 / skill |
| RN-10 | Saque não pode deixar a carteira negativa (saque > saldo é recusado). | ✓ Confirmada | skill CQRS |
| RN-11 | Depósito e saque exigem valor > 0. | ✓ Confirmada | skill CQRS |
| RN-1 | **Partida dobrada**: toda transação debita uma conta e credita outra (soma zero). | ✓ Confirmada | ADR-0008 |
| RN-4 | Correção só por estorno (evento oposto), nunca "editando" o passado. | ✓ Confirmada | ADR-0008 |
| RN-12 | Só se opera conta já aberta. | ✓ Confirmada | skill CQRS |
| RN-7 | Transação tem estado pendente (mutável) → postada (imutável). | ⚠ Confirmar | Modern Treasury |
| RN-13 | **Camada agêntica:** o agente consulta livremente (saldo, extrato); toda operação que move dinheiro exige confirmação explícita do titular na conversa; estorno NÃO é operável pelo agente. | ✓ Confirmada | Decisão 2026-09-02 |
| RN-14 | **Atores:** o único ator da v1 é o titular, na própria carteira. Estorno é operação interna — não exposta ao titular nem ao agente. | ✓ Confirmada | Decisão 2026-09-02 |
| RN-15 | **Idempotência:** toda operação que move dinheiro carrega chave de idempotência; pedido repetido com a mesma chave devolve o resultado original, sem criar lançamento novo. | ✓ Confirmada | Decisão 2026-09-02 |
| RN-16 | **Estorno vence o saldo:** o estorno é sempre aceito, mesmo que deixe a conta negativa. RN-10 restringe o SAQUE (ato voluntário), não a correção contábil. | ✓ Confirmada | Decisão 2026-09-02 |

> **Decidido (ADR-0008):** o FinAgent adota **partida dobrada** — agregado `Account`, evento
> `TransactionPosted` com lançamentos que somam zero, e uma conta de contrapartida do sistema
> (`ExternalFunding`). RN-1 e RN-4 viraram ✓. RN-7 (estados pendente/postado) segue ⚠: só
> vira lei se o FinAgent precisar de liquidação em duas fases — hoje a transação já nasce postada.

> **Decidido (2026-09-02, ao validar as specs BL-1..BL-7):** entram RN-13..RN-16.
> **RN-7 segue ⚠ e NÃO é adotada na v1** — sem integração bancária real, "pendente" não tem
> significado de negócio; a transação nasce postada. **Atenção à tensão RN-10 × RN-16:** saque
> nunca deixa o saldo negativo, estorno pode — a diferença é que saque é ato voluntário do
> titular e estorno é correção contábil, e recusá-lo deixaria os livros errados para sempre
> (violando RN-1 e RN-5). É exatamente o caso de "regra NOVA e explícita" que RN-10 exige.
> Fora do catálogo de propósito (ficam nas specs, por serem locais à feature): moeda BRL fixa,
> uma carteira por titular, estorno integral/com motivo/não re-estornável, paginação do extrato.
> A v1 **não coleta PII** (titular é identificador opaco) — se isso mudar, as LGPD-x ⚠ precisam
> ser confirmadas antes.

## Compliance / LGPD (transversal — qualquer feature com dado pessoal)

Nenhuma consta em ADR ainda — são recomendações a confirmar (arquitetura + jurídico).

| ID | Regra (enunciado do FinAgent) | Origem | Fonte |
|----|-------------------------------|--------|-------|
| LGPD-1 | Evento de domínio nunca carrega PII; só IDs/referências. | ⚠ Confirmar | LGPD art. 6 / Forgettable Payloads |
| LGPD-2 | Tornar PII de um titular inacessível a pedido, sem alterar eventos. | ⚠ Confirmar | LGPD art. 18, VI |
| LGPD-3 | PII no fluxo é cifrado com chave por titular; eliminar = destruir a chave. | ⚠ Confirmar | Crypto-Shredding (Verraes) |
| LGPD-4 | Dado financeiro é retido; elimina-se o PII vinculável, nunca o lançamento. | ⚠ Confirmar | LGPD art. 16, I |
| LGPD-5 | Dado anonimizado irreversível deixa de ser dado pessoal. | ⚠ Confirmar | LGPD art. 12 |
| LGPD-6 | Eliminação propaga para read models e projeções. | ⚠ Confirmar | event-driven.io |
| LGPD-7 | Tratamento de PII tem base legal; consentimento revogável dispara eliminação. | ⚠ Confirmar | LGPD art. 7 |
| LGPD-8 | PII cifrado em repouso e em transporte. | ⚠ Confirmar | LGPD art. 46 |
| LGPD-9 | Log, erro e payload Kafka não viram depósito de PII. | ⚠ Confirmar | LGPD art. 6 |

## Detalhe por módulo

- Wallet: `docs/modules/wallet/business-rules.md`
- Compliance: `docs/modules/compliance/business-rules.md`

## Como adicionar/alterar uma regra

1. Decida o enunciado (o parâmetro do FinAgent, não o princípio genérico).
2. Adicione a linha aqui com um ID, a Origem (✓/⚠) e a fonte.
3. Escreva o detalhe no arquivo do módulo. Módulo novo? Veja `docs/modules/README.md`.
4. Quando você confirmar uma regra ⚠, troque para ✓ e (se for de arquitetura) registre num ADR.
