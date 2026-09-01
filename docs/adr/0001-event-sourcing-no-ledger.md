# ADR-0001: Event Sourcing como fonte da verdade do ledger

- **Status:** Aceito
- **Data:** 2026-08-28

## Contexto
O núcleo do FinAgent é uma carteira financeira. Em domínio financeiro, saldo é
consequência de um histórico de movimentações, e auditabilidade não é opcional:
precisamos saber não só o estado atual, mas exatamente como se chegou nele.

## Decisão
Modelar a carteira com **Event Sourcing**. Cada movimentação (abertura, depósito,
saque) é persistida como um evento imutável. O saldo não é armazenado como campo
mutável — ele é derivado da reprodução (replay) dos eventos do agregado.

## Consequências
- (+) Auditoria e extrato saem de graça: os eventos *são* o ledger.
- (+) Permite reconstruir estados passados e depurar "como isso aconteceu".
- (+) Casa naturalmente com CQRS (ADR-0002) e mensageria (ADR-0004).
- (−) Maior complexidade que CRUD: versionamento de eventos, snapshots no futuro.
- (−) Consultas ad-hoc exigem um read model (resolvido no ADR-0002/0005).

## Alternativas consideradas
- **CRUD com tabela de saldo + tabela de transações:** mais simples, mas o saldo
  vira estado mutável sujeito a divergência do histórico, e perdemos o "porquê".
  Descartado por ser justamente o padrão que não demonstra a competência-alvo.
