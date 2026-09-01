# ADR-0008: Partida dobrada no ledger (double-entry)

- **Status:** Aceito
- **Data:** 2026-09-01

## Contexto
O ADR-0001 definiu Event Sourcing como fonte da verdade do ledger, mas não fixou o
*modelo contábil*. A implementação inicial tratava a carteira como uma **conta única com
saldo** (agregado `Wallet` emitindo `MoneyDeposited`/`MoneyWithdrawn`). Isso é, na prática,
um saldo com histórico — não um ledger. Um ledger de verdade é **partida dobrada**: todo
movimento de dinheiro tem dois lados que se cancelam (soma zero), o que dá a garantia de
que dinheiro nunca é criado nem some, e torna o sistema auditável de ponta a ponta.

## Decisão
Adotar **partida dobrada** no ledger:

- O agregado passa a ser a **`Account`** (a carteira do usuário é uma `Account` do tipo
  `Customer`). Existe uma **conta de contrapartida do sistema** (`ExternalFunding`), que
  representa o mundo externo (de onde o dinheiro entra / para onde sai).
- Toda movimentação é uma **transação com lançamentos balanceados** (`LedgerEntry`), em que
  a soma dos créditos é igual à soma dos débitos (**soma zero — RN-1**).
- A transação inteira é UM evento (`TransactionPosted`) — um único append. Assim os livros
  **sempre** fecham: é impossível persistir metade de uma transação.
- Saldo de cada conta é derivado dos lançamentos que a tocam (mantém o ADR-0001).
- Saldo insuficiente (RN-10) é validado dentro do agregado `Account` antes de postar.
- Correção é sempre por **estorno** — uma nova transação com os lados invertidos (RN-4),
  nunca alterando o passado (RN-3).

## Consequências
- (+) Garante a invariante de soma zero: dinheiro não é criado nem some (RN-1, RN-5, RN-6).
- (+) Transação atômica: como os dois lados moram em um só evento/append, os livros nunca
  ficam desbalanceados. Não precisa de saga para manter RN-1.
- (+) Eleva o projeto de "carteira com saldo" para "ledger de verdade" — auditável,
  contábil, mais próximo de um sistema financeiro real.
- (−) Modelo um pouco maior: aparece a `Account`, o `LedgerEntry` e a conta de
  contrapartida do sistema.
- (−) **Simplificação assumida:** a conta de contrapartida `ExternalFunding` não é um
  agregado próprio com regras — é uma conta virtual cujo saldo existe só na projeção
  (representa o mundo externo, sem restrição de saldo). Suficiente para carteira; se um dia
  o sistema precisar de contas internas com regra própria, revisitar.
- (−) Não adotamos plano de contas hierárquico, multi-moeda com conversão nem fechamento
  contábil — fora de escopo, marcados como decisões futuras.

## Alternativas consideradas
- **Manter conta única com saldo (partida simples):** mais simples, mas não é um ledger de
  verdade e não demonstra a competência-alvo; a nomenclatura "ledger" ficaria vazia.
  Descartado.
- **Uma conta por agregado, com os dois lados em streams separados:** seria "mais puro",
  mas exige coordenar dois appends (saga/outbox) para manter a soma zero — complexidade e
  risco de livros desbalanceados. Descartado em favor de um evento balanceado atômico.
