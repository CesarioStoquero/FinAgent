# Spec: Estornar uma transação

- **ID:** SPEC-2026-09-02-estornar-transacao  ·  **Status:** Validada  ·  **Validada em:** 2026-09-02
- **Autor:** business-analyst (FinAgent)  ·  **Data:** 2026-09-02
- **Origem:** BL-7 do `docs/backlog.md`  ·  **Módulo:** wallet

> Esta spec descreve O QUE o sistema deve fazer e POR QUÊ — nunca o COMO técnico.

## 1. Objetivo (por quê)

Erro operacional acontece: um depósito no valor errado, um saque indevido. O estorno é a
**única** forma de correção permitida (RN-4) — cria-se um movimento novo que anula o anterior,
e os dois ficam visíveis e encadeados no extrato. É a prova prática de que a imutabilidade
(RN-3) não é slogan: sem estorno, todo erro vira pedido para editar o passado.

## 2. Atores

- **Operação interna:** único ator que dispara o estorno (RN-14). Estorno não é exposto ao
  titular nem ao agente — estorno na mão de quem se beneficia dele é canal de fraude.
- **Titular:** afetado pelo estorno; vê o movimento e o motivo no extrato, mas não o dispara.
- **Sistema FinAgent:** registra o movimento de estorno ou recusa com motivo.
- **Contrapartida externa:** o outro lado da transação de estorno, fora da carteira do titular
  (ADR-0008).

## 3. Escopo

- **Entra:**
  - estornar **integralmente** uma transação já registrada de uma carteira (depósito ou saque),
    criando um movimento novo de mesmo valor e sentido oposto;
  - exigir **motivo obrigatório**, registrado e visível no extrato;
  - aceitar o estorno **mesmo que o saldo fique negativo** (RN-16);
  - tratar pedido repetido com a **mesma chave de idempotência** devolvendo o resultado
    original, sem criar movimento novo (RN-15);
  - manter o movimento original intacto e visível no extrato, encadeado ao estorno;
  - recusar, com motivo explícito, estorno inválido (transação inexistente, carteira
    inexistente, transação já estornada, tentativa de estornar um estorno).
- **NÃO entra:**
  - **estorno parcial de valor** — a v1 só estorna a transação inteira;
  - **estorno pelo titular ou pelo agente** — é operação interna (RN-13/RN-14);
  - **prazo limite para estornar** — não há; qualquer transação continua estornável;
  - editar, apagar ou ocultar o movimento original (proibido por RN-3);
  - novo lançamento com o valor correto (isso é depósito/saque normal, spec própria);
  - conversão de moeda; tarifas de estorno.

## 4. Cenários (comportamento observável)

- **Cenário feliz — estornar um depósito**
  - Dado uma carteira com saldo 10.000 centavos, proveniente de um único depósito de 10.000
    centavos
  - Quando a operação interna estorna esse depósito informando o motivo "depósito em
    duplicidade"
  - Então o saldo passa a ser 0 centavos, o extrato exibe dois movimentos (o depósito original,
    intacto, e o estorno de 10.000 centavos com o motivo) e o estorno referencia a transação
    estornada.

- **Cenário feliz — estornar um saque**
  - Dado uma carteira com saldo 7.500 centavos, após um depósito de 10.000 e um saque de 2.500
    centavos
  - Quando a operação interna estorna esse saque com motivo
  - Então o saldo volta a ser 10.000 centavos e o extrato exibe três movimentos: depósito,
    saque e estorno do saque.

- **Cenário feliz — o original nunca muda (RN-3)**
  - Dado uma transação já estornada
  - Quando o titular consulta o extrato
  - Então o movimento original aparece com o mesmo valor, a mesma data e o mesmo tipo de antes
    do estorno; nada nele foi alterado nem removido.

- **Borda 1 — estorno que deixa a carteira negativa é ACEITO (RN-16)**
  - Dado uma carteira em que o depósito de 10.000 centavos já foi integralmente sacado, com
    saldo 0
  - Quando a operação interna estorna aquele depósito
  - Então o estorno é **aceito**, o saldo passa a ser **-10.000 centavos**, e o extrato exibe o
    saldo acumulado negativo. A recusa por saldo (RN-10) vale para o saque, não para o estorno.

- **Borda 2 — transação já estornada**
  - Dado uma transação que já foi estornada
  - Quando se pede o estorno dela outra vez, com chave de idempotência diferente
  - Então a operação é recusada com o motivo "transação já estornada"; nenhum movimento novo é
    registrado.

- **Borda 3 — estornar um estorno**
  - Dado um movimento que é ele próprio um estorno
  - Quando se pede o estorno desse movimento
  - Então a operação é recusada com o motivo "estorno não é estornável".

- **Borda 4 — pedido repetido com a mesma chave de idempotência (RN-15)**
  - Dado um estorno já aceito com uma determinada chave de idempotência
  - Quando chega outro pedido de estorno com a mesma chave
  - Então o sistema devolve o resultado do estorno original, o saldo não muda e o extrato
    continua com exatamente um movimento de estorno.

- **Borda 5 — motivo ausente**
  - Dado uma transação estornável
  - Quando se pede o estorno sem informar motivo
  - Então a operação é recusada com o motivo "motivo obrigatório"; nada é registrado.

- **Borda 6 — tentar editar em vez de estornar (RN-4)**
  - Dado uma transação registrada com valor errado
  - Quando se tenta corrigir alterando o valor da transação original
  - Então a tentativa é recusada com o motivo "correção só por estorno"; o histórico permanece
    intacto.

- **Borda 7 — transação inexistente**
  - Dado um identificador de transação que não existe naquela carteira
  - Quando se tenta estorná-lo
  - Então o estorno é recusado com o motivo "transação inexistente" e nada é registrado.

- **Borda 8 — carteira inexistente (RN-12)**
  - Dado um identificador de carteira que nunca foi aberto
  - Quando se tenta estornar uma transação nele
  - Então a operação é recusada com o motivo "carteira inexistente".

- **Borda 9 — sem prazo limite**
  - Dado uma transação registrada há vários anos
  - Quando a operação interna a estorna com motivo
  - Então o estorno é aceito: não há prazo de expiração para a correção contábil.

- **Borda 10 — titular e agente não estornam (RN-13/RN-14)**
  - Dado o titular pedindo ao agente "estorna aquele depósito"
  - Quando o agente processa o pedido
  - Então nenhum estorno é registrado: o agente informa que estorno não é operação disponível
    para o titular; a mesma tentativa feita diretamente pelo titular também é recusada.

- **Borda 11 — soma zero do estorno (RN-1)**
  - Dado um estorno aceito de 10.000 centavos
  - Quando se somam os dois lados da transação de estorno
  - Então o resultado é 0: o estorno também não cria nem faz sumir dinheiro.

- **Borda 12 — mesmo valor e mesma moeda do original**
  - Dado uma transação de 10.000 centavos em BRL
  - Quando ela é estornada
  - Então o estorno tem exatamente 10.000 centavos, em BRL, com sentido oposto ao do movimento
    original.

- **Borda 13 — estorno recusado não deixa rastro**
  - Dado uma carteira com saldo 7.500 centavos
  - Quando um estorno é recusado por qualquer motivo
  - Então o saldo continua 7.500 centavos e nenhum movimento novo aparece no extrato.

- **Borda 14 — o próprio estorno é imutável (RN-3)**
  - Dado um estorno já registrado
  - Quando se tenta alterá-lo ou removê-lo
  - Então a tentativa é recusada.

## 5. Requisitos funcionais

- **RF-1:** O sistema DEVE permitir que uma operação interna estorne uma transação já
  registrada de uma carteira, criando um movimento novo de valor igual e sentido oposto ao
  original (RN-4).
- **RF-2:** O estorno DEVE ser sempre **integral** — o valor total da transação original.
- **RF-3:** O sistema DEVE exigir **motivo** no pedido de estorno, recusando o pedido sem
  motivo, e DEVE exibir esse motivo no extrato.
- **RF-4:** O sistema DEVE **aceitar** o estorno ainda que ele deixe o saldo da carteira
  negativo (RN-16); a recusa por saldo insuficiente (RN-10) NÃO se aplica ao estorno.
- **RF-5:** O movimento original NÃO DEVE ser alterado, removido nem ocultado do extrato em
  nenhuma hipótese (RN-3).
- **RF-6:** O estorno DEVE aparecer no extrato como movimento próprio, identificado como
  estorno e referenciando a transação que anula (RN-4).
- **RF-7:** Após o estorno, o saldo DEVE ser igual ao saldo que a carteira teria se o movimento
  original não tivesse existido, considerando os demais movimentos ocorridos.
- **RF-8:** O sistema DEVE recusar o estorno de uma transação **já estornada**, informando o
  motivo.
- **RF-9:** O sistema DEVE recusar o estorno de um movimento que é ele próprio um **estorno**,
  informando o motivo.
- **RF-10:** O sistema DEVE exigir chave de idempotência no estorno e, para pedido repetido com
  a mesma chave, DEVE devolver o resultado do estorno original sem criar movimento novo (RN-15).
- **RF-11:** O sistema DEVE recusar qualquer tentativa de corrigir uma transação por edição ou
  remoção, informando que a correção só acontece por estorno (RN-3/RN-4).
- **RF-12:** O sistema DEVE recusar estorno de transação que não existe na carteira informada.
- **RF-13:** O sistema DEVE recusar estorno em carteira inexistente (RN-12).
- **RF-14:** O sistema NÃO DEVE expor o estorno ao titular nem ao agente de linguagem natural;
  toda tentativa por esses atores é recusada (RN-13/RN-14).
- **RF-15:** O sistema NÃO DEVE aplicar prazo limite ao estorno: transação de qualquer data
  continua estornável.
- **RF-16:** A transação de estorno DEVE somar zero: o valor que entra em um lado sai do outro
  (RN-1).
- **RF-17:** O estorno DEVE usar exatamente o mesmo valor, em centavos inteiros, e a mesma
  moeda (BRL) da transação original (RN-8/RN-9).
- **RF-18:** Um estorno recusado NÃO DEVE alterar o saldo nem acrescentar movimento ao extrato.
- **RF-19:** O próprio estorno, uma vez registrado, DEVE ser imutável como qualquer outro
  movimento (RN-3).

## 6. Regras de negócio (invariantes do domínio)

- **Correção só por estorno:** valor registrado não se edita; corrige-se acrescentando um
  movimento oposto, encadeado ao original (RN-4).
- **Imutabilidade:** nem o original nem o estorno mudam ou desaparecem depois de registrados
  (RN-3).
- **Estorno vence o saldo:** o estorno é sempre aceito, mesmo deixando a carteira negativa
  (RN-16) — recusá-lo deixaria os livros errados para sempre, violando RN-1 e RN-5. RN-10
  protege o **saque**, que é ato voluntário; o estorno é correção contábil.
- **Integralidade:** estorno é do valor total da transação; não há estorno parcial na v1.
- **Unicidade:** cada transação é estornável no máximo uma vez, e um estorno não é estornável.
- **Motivo:** todo estorno carrega motivo, visível no extrato.
- **Ator:** estorno é operação interna, nunca do titular nem do agente (RN-13/RN-14).
- **Idempotência:** mesma chave = mesmo resultado, nunca um segundo lançamento (RN-15).
- **Soma zero:** o estorno é transação de dois lados que somam zero (RN-1) — devolve o valor ao
  lado de onde veio.
- **Dinheiro:** valores em centavos inteiros, idênticos ao original, sem arredondamento (RN-8).
- **Moeda:** BRL, a mesma do original e da carteira (RN-9).
- **Existência:** só se estorna transação existente, de carteira já aberta (RN-12).
- **Rastreabilidade:** depois do estorno, o extrato continua explicando cada centavo do saldo,
  agora com dois movimentos (RN-6).

## 7. Restrições herdadas (ADR-x / RN-x)

| ID | O que impõe A ESTA feature | Fonte |
|----|----------------------------|-------|
| RN-4 ✓ | Esta é a única forma de correção permitida: novo movimento oposto, encadeado ao original — nunca edição do passado. | `docs/business-rules.md` |
| RN-3 ✓ | O movimento original permanece intacto e visível; o próprio estorno também é definitivo depois de registrado. | `docs/business-rules.md` |
| RN-1 ✓ | A transação de estorno tem dois lados de mesmo valor que somam zero. | `docs/business-rules.md` |
| RN-16 ✓ | O estorno é SEMPRE aceito, ainda que deixe a carteira negativa — e **RN-10 (saldo não-negativo) restringe o SAQUE, não o estorno**. | `docs/business-rules.md` |
| RN-14 ✓ | Estorno é operação interna: não é exposto ao titular nem a nenhum outro ator da v1. | `docs/business-rules.md` |
| RN-13 ✓ | Estorno NÃO é operável pelo agente de linguagem natural, em nenhuma circunstância — nem com confirmação. | `docs/business-rules.md` |
| RN-15 ✓ | O estorno carrega chave de idempotência; pedido repetido com a mesma chave devolve o resultado original e não cria movimento novo. | `docs/business-rules.md` |
| RN-8 ✓ *(invariante geral do módulo)* | O valor estornado é inteiro de centavos, idêntico ao do original. | `docs/business-rules.md` |
| RN-9 ✓ *(invariante geral do módulo)* | O estorno é em BRL, a mesma moeda do original e da carteira; sem conversão. | `docs/business-rules.md` |
| RN-12 ✓ *(herdada de BL-5)* | Só se estorna transação de carteira já aberta. | `docs/business-rules.md` |
| ADR-0001 | O estorno é um fato novo acrescentado ao histórico; corrigir jamais significa reescrever o histórico. | `docs/adr/0001-event-sourcing-no-ledger.md` |
| ADR-0008 | O estorno é transação balanceada com os lados invertidos em relação à original, registrada de forma inteira; carteira do titular e contrapartida externa continuam sendo os dois lados. | `docs/adr/0008-partida-dobrada-no-ledger.md` |

> Todas as regras acima são ✓ (lei) e viraram RF-x e critério de aceite. Nenhuma regra ⚠
> entra aqui — RN-7 segue ⚠ e não foi adotada na v1 (ver seção 9).

## 8. Camada agêntica (linguagem natural)

**Não se aplica.** Por RN-13, o estorno **não é operável pelo agente**; por RN-14, ele é
operação interna, fora do alcance do titular. Se o titular pedir um estorno em conversa, o
agente informa que a operação não está disponível para ele e **não** registra nada (cenário
Borda 10).

## 9. Suposições e fora de escopo

- **Suposição:** estornar não recoloca o valor "correto"; se a intenção era corrigir um valor,
  o lançamento correto é uma nova operação de depósito ou saque, feita à parte.
- **Decisão registrada (RN-7 ⚠ não adotada na v1):** a transação a estornar já é definitiva;
  não existe estado "pendente" a cancelar antes de postar.
- **Suposição:** o motivo do estorno é texto curto, exibido como descrição daquele movimento no
  extrato; ele não contém dado pessoal (a v1 não coleta PII).
- **Fora de escopo agora:** estorno parcial, estorno em lote, re-estorno, prazo limite, tarifa
  de estorno, fluxo de aprovação/contestação, notificação ao titular.

## 10. Pendências de decisão

Nenhuma. Todas as pendências foram decididas em 2026-09-02.

## 11. Critérios de aceite

- [ ] Estornar o único depósito de 10.000 centavos leva o saldo a 0 e deixa dois movimentos no
      extrato.
- [ ] Estornar um saque de 2.500 centavos devolve o saldo ao valor anterior ao saque.
- [ ] Estornar um depósito cujo valor já foi sacado é **aceito** e deixa o saldo em -10.000
      centavos (RN-16).
- [ ] Estorno de transação já estornada é recusado com motivo "transação já estornada".
- [ ] Estorno de um movimento que é um estorno é recusado com motivo "estorno não é
      estornável".
- [ ] Pedido repetido com a mesma chave de idempotência devolve o resultado original e não cria
      movimento novo (RN-15).
- [ ] Estorno sem motivo é recusado; estorno com motivo exibe esse motivo no extrato.
- [ ] Após o estorno, o movimento original aparece no extrato com valor, data e tipo
      inalterados (RN-3).
- [ ] O estorno aparece no extrato identificado como estorno e referenciando a transação
      anulada (RN-4).
- [ ] Tentativa de corrigir uma transação por edição ou remoção é recusada com o motivo
      "correção só por estorno" (RN-3/RN-4).
- [ ] Estorno de transação inexistente é recusado com motivo "transação inexistente".
- [ ] Estorno em carteira inexistente é recusado com motivo "carteira inexistente" (RN-12).
- [ ] Pedido de estorno feito pelo titular ou pelo agente é recusado e nada é registrado
      (RN-13/RN-14).
- [ ] Transação antiga (anos) continua estornável — não há prazo limite.
- [ ] Para todo estorno aceito, a soma dos dois lados da transação é 0 (RN-1).
- [ ] O estorno tem exatamente o mesmo valor em centavos e a mesma moeda BRL do original
      (RN-8/RN-9).
- [ ] Tentativa de alterar ou remover um estorno registrado é recusada (RN-3).
- [ ] Após um estorno recusado, saldo e extrato permanecem exatamente como antes.
- [ ] O saldo consultado após o estorno continua igual à soma dos movimentos do extrato.
