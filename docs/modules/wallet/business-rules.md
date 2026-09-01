# Regras de negócio — Módulo Wallet / Ledger

Fonte da verdade de DOMÍNIO do módulo. Descreve regras de negócio (o que é sempre
verdade no mundo real), não implementação. Specs, testes e código referenciam pelo ID
(RN-x). Baseado em contabilidade de partidas dobradas e design de ledger imutável
(fontes no fim).

> Status de cada regra (✓/⚠) está no catálogo central `docs/business-rules.md`.
> **Partida dobrada foi adotada (ADR-0008):** RN-1 e RN-4 agora são ✓. O modelo é agregado
> `Account` + evento `TransactionPosted` (lançamentos que somam zero) + conta de
> contrapartida do sistema `ExternalFunding`. Só RN-7 (estados pendente/postado) segue ⚠.

> Como isto casa com o FinAgent: o Event Sourcing do ledger (ADR-0001) É a materialização
> destas regras — eventos append-only = ledger imutável; saldo derivado do replay = saldo
> derivado dos lançamentos. Estas regras são o "porquê" por trás daqueles ADRs.

## Conceitos

- **Conta (Account):** onde valor repousa (ex.: a carteira do usuário, uma conta de
  contrapartida do sistema).
- **Lançamento (Entry):** um débito OU um crédito em uma conta, com valor em centavos.
- **Transação (Transaction):** um conjunto de lançamentos que, juntos, movem valor.
- **Saldo (Balance):** derivado da soma dos lançamentos de uma conta.

## Invariantes do ledger (NUNCA violáveis)

- **RN-1 (Partida dobrada / soma zero):** toda transação DEVE debitar uma conta e
  creditar outra pelo mesmo valor. A soma dos lançamentos de uma transação é sempre 0 —
  cada centavo que sai de um lugar entra em outro. Nada de "criar" ou "sumir" dinheiro.
- **RN-2 (Saldo é derivado, nunca armazenado como verdade):** o saldo de uma conta é a
  soma dos seus lançamentos. NUNCA é uma coluna mutável que se sobrescreve.
- **RN-3 (Imutabilidade do postado):** um lançamento/transação, uma vez **postado**,
  NUNCA muda nem é apagado. Sem UPDATE, sem DELETE em lançamento postado.
- **RN-4 (Correção só por estorno):** para corrigir um valor postado, NÃO edite —
  crie uma nova transação que estorna (valor oposto) e, se preciso, outra com o valor
  correto. O histórico de ajustes fica auditável e encadeado ao mesmo objeto de negócio.
- **RN-5 (Completude):** nenhum movimento de dinheiro acontece sem um lançamento
  correspondente. Se o valor se moveu, existe um registro que o explica.
- **RN-6 (Rastreabilidade):** deve ser possível explicar cada centavo do saldo atual
  percorrendo a sequência de lançamentos, sem lacunas.

## Estados de uma transação

- **RN-7:** uma transação é **mutável enquanto pendente** e **imutável depois de postada**.
  (Espelha prazos bancários reais: ACH/TED levam dias.) Falhas vão para **arquivada**,
  não são apagadas.

## Regras específicas da carteira

- **RN-8 (Dinheiro em centavos):** todo valor é inteiro de centavos (`long`). Proibido
  fração de centavo e ponto flutuante.
- **RN-9 (Moeda única por operação):** operar entre moedas diferentes é proibido; a
  conversão, se existir, é uma transação explícita com taxa, não uma soma direta.
- **RN-10 (Saldo não-negativo por padrão):** um saque não pode deixar a carteira
  negativa. Saque > saldo é recusado. (Se algum produto permitir cheque especial, isso
  é uma regra NOVA e explícita, nunca o default.)
- **RN-11 (Valores positivos):** depósito e saque exigem valor > 0.
- **RN-12 (Carteira existe antes de operar):** só se deposita/saca de uma carteira já
  aberta.

## Como usar estas regras

- **Spec (`nova-spec`):** ao descrever cenários de borda, cubra as RN aplicáveis
  (RN-10 saldo insuficiente, RN-9 moeda divergente, RN-11 valor <= 0).
- **Plano (`software-engineer`):** valide que a abordagem não fere nenhuma RN.
- **Implementação:** cada RN de borda vira um teste (ver skill `dotnet-testing`).
- **Revisão:** o `wallet-specialist` confere o código contra RN-1..RN-12 por ID.

## Fontes

- Modern Treasury — *Enforcing Immutability in your Double-Entry Ledger* (estorno vs
  edição; pendente mutável → postado imutável; saldo derivado).
- Modern Treasury — *How to Scale a Ledger, Part V: Immutability and Double-Entry*.
- Square Developer Blog — *Books: an immutable double-entry accounting database*.
- Formance — *Immutable ledgers: append-only data models*.
- Griffin — *Building an immutable bank* (soma zero, sequência sem lacunas,
  correção não-destrutiva, verificabilidade independente).
