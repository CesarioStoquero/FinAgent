# Spec: Depositar dinheiro na carteira

- **ID:** SPEC-2026-09-02-depositar-na-carteira  ·  **Status:** Validada  ·  **Validada em:** 2026-09-02
- **Autor:** business-analyst (FinAgent)  ·  **Data:** 2026-09-02
- **Origem:** BL-1 do `docs/backlog.md`  ·  **Módulo:** wallet

> Esta spec descreve O QUE o sistema deve fazer e POR QUÊ — nunca o COMO técnico.

## 1. Objetivo (por quê)

É a primeira entrada de dinheiro no FinAgent: sem depósito não há saldo para consultar
(BL-2) nem histórico para auditar (BL-3). O depósito registra o movimento como fato
definitivo — depois de aceito, ele não é editado nem apagado; se estiver errado, a correção é
um estorno (BL-7).

## 2. Atores

- **Titular:** único ator da v1 (RN-14); deposita apenas na própria carteira.
- **Sistema FinAgent:** valida, registra o movimento e passa a refletir o novo saldo.
- **Contrapartida externa:** o "de onde o dinheiro veio" — o outro lado da transação, fora da
  carteira do titular (ADR-0008).
- **Agente de linguagem natural:** pede o depósito em conversa, sempre com confirmação
  explícita do titular (RN-13, ver seção 8).

## 3. Escopo

- **Entra:**
  - registrar um depósito de valor positivo, em centavos inteiros, em BRL, numa carteira
    aberta;
  - aceitar uma **descrição livre opcional** (texto curto) no depósito, exibida no extrato;
  - tratar pedido repetido com a **mesma chave de idempotência** devolvendo o resultado
    original, sem criar movimento novo (RN-15);
  - recusar, com motivo explícito, depósito inválido (valor não positivo, carteira
    inexistente, moeda divergente);
  - deixar o depósito aceito visível no histórico e refletido no saldo;
  - garantir que o depósito registrado nunca seja alterado nem removido.
- **NÃO entra:**
  - corrigir/cancelar um depósito (isso é estorno — `docs/specs/estornar-transacao/spec.md`);
  - **valor máximo por depósito ou por período** — a v1 não tem limite;
  - **estado "pendente"/prazo de compensação** — a transação nasce definitiva (RN-7 não
    adotada);
  - depósito feito por terceiro em carteira alheia (RN-14);
  - abrir a carteira, sacar, consultar saldo e consultar extrato (specs próprias);
  - conversão de moeda, tarifas, depósito agendado ou recorrente.

## 4. Cenários (comportamento observável)

- **Cenário feliz — depósito aceito**
  - Dado uma carteira aberta com saldo 0 centavos
  - Quando o titular deposita 10.000 centavos em BRL
  - Então o depósito é aceito, o saldo passa a ser 10.000 centavos e o extrato passa a exibir
    exatamente um movimento de entrada de 10.000 centavos.

- **Cenário feliz — depósitos sucessivos somam**
  - Dado uma carteira aberta com saldo 10.000 centavos
  - Quando o titular deposita mais 2.550 centavos
  - Então o saldo passa a ser 12.550 centavos e o extrato exibe dois movimentos de entrada.

- **Cenário feliz — depósito com descrição**
  - Dado uma carteira aberta
  - Quando o titular deposita 5.000 centavos com a descrição "salário"
  - Então o depósito é aceito e o extrato exibe esse movimento com a descrição "salário".

- **Cenário feliz — depósito sem descrição**
  - Dado uma carteira aberta
  - Quando o titular deposita 5.000 centavos sem informar descrição
  - Então o depósito é aceito e o extrato exibe o movimento sem descrição, apenas com tipo,
    data/hora, valor e moeda.

- **Borda 1 — pedido repetido com a mesma chave de idempotência (RN-15)**
  - Dado um depósito de 10.000 centavos já aceito com uma determinada chave de idempotência
  - Quando chega outro pedido de depósito com a mesma chave
  - Então o sistema devolve o resultado do depósito original, o saldo continua 10.000 centavos
    e o extrato continua com exatamente um movimento.

- **Borda 2 — valor zero (RN-11)**
  - Dado uma carteira aberta
  - Quando o titular tenta depositar 0 centavos
  - Então o depósito é recusado com o motivo "valor deve ser maior que zero"; saldo e extrato
    permanecem inalterados.

- **Borda 3 — valor negativo (RN-11)**
  - Dado uma carteira aberta
  - Quando o titular tenta depositar -500 centavos
  - Então o depósito é recusado com o motivo "valor deve ser maior que zero"; nada é
    registrado.

- **Borda 4 — valor com fração de centavo (RN-8)**
  - Dado uma carteira aberta
  - Quando chega um pedido de depósito de valor que não é inteiro de centavos (ex.: 10,555)
  - Então o depósito é recusado; o sistema não arredonda nem trunca.

- **Borda 5 — carteira inexistente (RN-12)**
  - Dado um identificador de carteira que nunca foi aberto
  - Quando alguém tenta depositar nele
  - Então o depósito é recusado com o motivo "carteira inexistente" e nada é registrado.

- **Borda 6 — moeda divergente (RN-9)**
  - Dado uma carteira aberta em BRL
  - Quando o titular tenta depositar um valor expresso em outra moeda
  - Então o depósito é recusado com o motivo "moeda divergente"; o sistema não converte nem
    soma valores de moedas diferentes.

- **Borda 7 — carteira de outro titular (RN-14)**
  - Dado um titular e a carteira de outro titular
  - Quando ele tenta depositar na carteira alheia
  - Então a operação é recusada; na v1 só o titular deposita na própria carteira.

- **Borda 8 — tentativa de alterar o passado (RN-3)**
  - Dado um depósito já registrado
  - Quando se tenta alterar o valor desse movimento ou removê-lo do histórico
  - Então a tentativa é recusada; o movimento original continua no extrato exatamente como foi
    registrado, e a única correção possível é um estorno (BL-7).

- **Borda 9 — confirmação na conversa (RN-13)**
  - Dado o titular dizendo ao agente "põe 50 reais na minha carteira"
  - Quando o agente entende a intenção
  - Então ele apresenta a operação e só registra o depósito após confirmação explícita do
    titular; sem confirmação, nada é registrado.

- **Borda 10 — soma zero da transação (RN-1)**
  - Dado um depósito aceito de 10.000 centavos
  - Quando se somam os dois lados da transação (entrada na carteira e saída da contrapartida
    externa)
  - Então o resultado é 0: nenhum centavo é criado nem some.

- **Borda 11 — recusa não deixa rastro parcial**
  - Dado uma carteira com saldo 10.000 centavos
  - Quando um depósito é recusado por qualquer motivo
  - Então o saldo continua 10.000 centavos e nenhum movimento novo aparece no extrato.

## 5. Requisitos funcionais

- **RF-1:** O sistema DEVE registrar um depósito de valor maior que zero, em centavos
  inteiros, em BRL, numa carteira aberta, aumentando o saldo exatamente no valor depositado.
- **RF-2:** O sistema DEVE aceitar uma descrição livre opcional (texto curto) no depósito e
  exibi-la no extrato; sem descrição, o movimento é registrado normalmente.
- **RF-3:** O sistema DEVE exigir chave de idempotência no depósito e, para pedido repetido
  com a mesma chave, DEVE devolver o resultado do depósito original sem criar movimento novo
  (RN-15).
- **RF-4:** O sistema DEVE recusar depósito com valor menor ou igual a zero (RN-11).
- **RF-5:** O sistema DEVE recusar depósito cujo valor não seja inteiro de centavos, sem
  arredondar nem truncar (RN-8).
- **RF-6:** O sistema DEVE recusar depósito dirigido a carteira inexistente (RN-12).
- **RF-7:** O sistema DEVE recusar depósito expresso em moeda diferente de BRL, sem converter
  (RN-9).
- **RF-8:** O sistema DEVE recusar depósito em carteira que não é a do titular solicitante
  (RN-14).
- **RF-9:** Todo depósito aceito DEVE produzir exatamente um registro correspondente no
  histórico da carteira (RN-5).
- **RF-10:** Todo depósito aceito DEVE ter contrapartida de mesmo valor fora da carteira, de
  modo que a transação some zero (RN-1).
- **RF-11:** Um depósito registrado NÃO DEVE ser alterado nem removido; toda tentativa é
  recusada (RN-3).
- **RF-12:** Um depósito recusado NÃO DEVE alterar o saldo nem acrescentar movimento ao
  extrato.
- **RF-13:** O saldo após o depósito DEVE continuar igual à soma de todos os movimentos da
  carteira (RN-2).
- **RF-14:** O depósito aceito DEVE ser identificável de forma única no extrato, para poder
  ser referenciado por um estorno (BL-7).
- **RF-15:** Quando pedido pelo agente, o depósito só DEVE ser registrado após confirmação
  explícita do titular na conversa (RN-13).
- **RF-16:** O depósito NÃO DEVE ter valor máximo na v1: qualquer valor positivo em centavos
  inteiros é aceito.

## 6. Regras de negócio (invariantes do domínio)

- **Dinheiro:** todo valor é inteiro de centavos (`long`); proibido ponto flutuante e fração
  de centavo (RN-8).
- **Valor:** depósito exige valor estritamente maior que zero (RN-11); sem teto na v1.
- **Moeda:** BRL; moedas diferentes não se somam e não são convertidas (RN-9).
- **Existência:** só se deposita em carteira já aberta (RN-12).
- **Ator:** só o titular deposita na própria carteira (RN-14).
- **Idempotência:** mesma chave = mesmo resultado, nunca um segundo lançamento (RN-15).
- **Completude:** nenhum dinheiro se move sem registro correspondente (RN-5).
- **Imutabilidade:** movimento registrado não muda e não é apagado (RN-3); correção só por
  estorno (RN-4, ver BL-7).
- **Soma zero:** toda transação tem dois lados de mesmo valor e soma zero (RN-1).
- **Saldo:** derivado da soma dos movimentos, nunca mantido à parte (RN-2).

## 7. Restrições herdadas (ADR-x / RN-x)

| ID | O que impõe A ESTA feature | Fonte |
|----|----------------------------|-------|
| RN-11 ✓ | Depósito só é aceito com valor estritamente maior que zero; 0 e negativos são recusados. | `docs/business-rules.md` |
| RN-5 ✓ | Todo depósito aceito gera registro correspondente: não existe valor que se move sem registro. | `docs/business-rules.md` |
| RN-3 ✓ | Depósito registrado é definitivo: não se altera nem se apaga; a única correção é estorno. | `docs/business-rules.md` |
| RN-1 ✓ | O depósito é uma transação de dois lados que somam zero: entra na carteira o mesmo valor que sai da contrapartida externa. | `docs/business-rules.md` |
| RN-8 ✓ | O valor depositado é inteiro de centavos; nada de fração nem arredondamento. | `docs/business-rules.md` |
| RN-9 ✓ | Depósito em moeda diferente da moeda da carteira (BRL) é recusado; sem conversão. | `docs/business-rules.md` |
| RN-12 ✓ *(herdada de BL-5)* | Só se deposita em carteira já aberta. | `docs/business-rules.md` |
| RN-13 ✓ | Depósito move dinheiro: pedido pelo agente exige confirmação explícita do titular antes de ser efetivado. | `docs/business-rules.md` |
| RN-14 ✓ | O titular deposita apenas na própria carteira; não há depósito de terceiro nem operação por outro ator. | `docs/business-rules.md` |
| RN-15 ✓ | O depósito carrega chave de idempotência; pedido repetido com a mesma chave devolve o resultado original e não cria movimento novo. | `docs/business-rules.md` |
| ADR-0001 | O histórico é a fonte da verdade e o saldo é derivado dele; o depósito é fato acrescentado, nunca sobrescrita de saldo. | `docs/adr/0001-event-sourcing-no-ledger.md` |
| ADR-0008 | O depósito é transação balanceada entre a carteira do titular (conta `Customer`) e a contrapartida externa; é registrada inteira — nunca metade dela. | `docs/adr/0008-partida-dobrada-no-ledger.md` |

> Todas as regras acima são ✓ (lei) e viraram RF-x e critério de aceite. Nenhuma regra ⚠
> entra aqui — RN-7 (pendente → postada) segue ⚠ e não foi adotada na v1 (ver seção 9).

## 8. Camada agêntica (linguagem natural)

Como **tool MCP**, esta operação é "depositar na carteira": o titular diz algo como "põe 50
reais na minha carteira".

- **Confirmação obrigatória (RN-13):** por mover dinheiro, o agente apresenta a operação
  (valor, moeda, carteira) e só a efetiva após confirmação explícita do titular na conversa.
  Sem confirmação, nada é registrado.
- **Autorização na borda da tool (RN-14):** o agente só deposita na carteira do próprio
  titular da conversa.
- **Repetição (RN-15):** reexecução do mesmo pedido, com a mesma chave de idempotência,
  devolve o depósito original — retry de conversa não duplica dinheiro.

## 9. Suposições e fora de escopo

- **Decisão registrada (RN-7 ⚠ não adotada na v1):** o depósito aceito já nasce definitivo;
  não existe estado "pendente" nem prazo de liberação — o valor fica imediatamente disponível
  para saque.
- **Suposição:** a origem do dinheiro é a contrapartida externa genérica do sistema; o
  depósito não identifica pagador (a v1 não coleta PII).
- **Suposição:** a descrição livre é texto curto sem significado para o cálculo do saldo —
  serve para o titular reconhecer o movimento no extrato.
- **Fora de escopo agora:** limite de valor, tarifas, prazo de compensação, depósito agendado
  ou recorrente, depósito em moeda diferente com conversão.

## 10. Pendências de decisão

Nenhuma. Todas as pendências foram decididas em 2026-09-02.

## 11. Critérios de aceite

- [ ] Depósito de valor positivo em carteira aberta é aceito e aumenta o saldo exatamente
      naquele valor.
- [ ] Após dois depósitos, o saldo é a soma dos dois valores e o extrato exibe os dois
      movimentos.
- [ ] Depósito com descrição exibe a descrição no extrato; depósito sem descrição é aceito
      normalmente.
- [ ] Pedido repetido com a mesma chave de idempotência devolve o resultado original, mantém
      o saldo e não cria movimento novo (RN-15).
- [ ] Depósito de 0 ou de valor negativo é recusado com motivo "valor deve ser maior que
      zero" (RN-11).
- [ ] Depósito com valor que não é inteiro de centavos é recusado, sem arredondamento (RN-8).
- [ ] Depósito em carteira inexistente é recusado com motivo "carteira inexistente" (RN-12).
- [ ] Depósito em moeda diferente de BRL é recusado com motivo "moeda divergente", sem
      conversão (RN-9).
- [ ] Depósito em carteira de outro titular é recusado (RN-14).
- [ ] Depósito solicitado pelo agente sem confirmação explícita do titular não registra nada;
      com confirmação, registra (RN-13).
- [ ] Toda tentativa de alterar ou remover um depósito registrado é recusada e o extrato
      permanece idêntico (RN-3).
- [ ] Para todo depósito aceito, a soma dos dois lados da transação é 0 (RN-1).
- [ ] Após qualquer depósito recusado, saldo e extrato permanecem exatamente como antes.
- [ ] O saldo consultado é sempre igual à soma dos movimentos do extrato.
