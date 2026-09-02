# Spec: Consultar o saldo da carteira

- **ID:** SPEC-2026-09-02-consultar-saldo  ·  **Status:** Validada  ·  **Validada em:** 2026-09-02
- **Autor:** business-analyst (FinAgent)  ·  **Data:** 2026-09-02
- **Origem:** BL-2 do `docs/backlog.md`  ·  **Módulo:** wallet

> Esta spec descreve O QUE o sistema deve fazer e POR QUÊ — nunca o COMO técnico.

## 1. Objetivo (por quê)

"Quanto eu tenho?" é a pergunta mais frequente do produto. A resposta precisa ser a
consequência dos movimentos registrados, não um número mantido à parte: saldo guardado como
valor mutável diverge do histórico e destrói a auditabilidade prometida (RN-2).

## 2. Atores

- **Titular:** único ator da v1 (RN-14); consulta apenas a própria carteira.
- **Sistema FinAgent:** responde com o valor derivado dos movimentos da carteira.
- **Agente de linguagem natural:** consulta livremente, sem confirmação (RN-13, ver seção 8).

## 3. Escopo

- **Entra:**
  - consultar o saldo atual de uma carteira aberta;
  - devolver o saldo em centavos inteiros, em BRL;
  - devolver a **data/hora do último movimento considerado** naquele saldo;
  - recusar, com motivo explícito, consulta a carteira inexistente ou de outro titular;
  - garantir que o saldo informado é sempre reconciliável com os movimentos do extrato.
- **NÃO entra:**
  - listar os movimentos (isso é extrato — `docs/specs/consultar-extrato/spec.md`);
  - saldo em data passada, saldo projetado, saldo consolidado de várias carteiras;
  - **distinção entre saldo disponível e saldo pendente** — não há transação pendente na v1
    (RN-7 não adotada);
  - qualquer operação que altere o saldo (specs próprias);
  - conversão de moeda.

## 4. Cenários (comportamento observável)

- **Cenário feliz — saldo de carteira com movimentos**
  - Dado uma carteira aberta com um depósito de 10.000 centavos e um saque de 2.500 centavos
  - Quando o titular consulta o saldo
  - Então recebe 7.500 centavos, em BRL, acompanhados da data/hora do último movimento
    considerado (a do saque).

- **Cenário feliz — carteira sem movimentos**
  - Dado uma carteira recém-aberta, sem nenhum movimento
  - Quando o titular consulta o saldo
  - Então recebe 0 centavos, e a resposta indica que não há movimento considerado.

- **Borda 1 — saldo é derivado, não guardado (RN-2)**
  - Dado uma carteira com uma sequência qualquer de movimentos
  - Quando se somam, um a um, todos os movimentos exibidos no extrato dessa carteira
  - Então o resultado é exatamente igual ao saldo informado na consulta — sem diferença de um
    centavo.

- **Borda 2 — saldo acompanha o estorno**
  - Dado uma carteira com um depósito de 10.000 centavos já estornado
  - Quando o titular consulta o saldo
  - Então recebe 0 centavos: o estorno entra na conta como movimento próprio e o depósito
    original continua visível no extrato.

- **Borda 3 — saldo negativo por estorno é exibido como está (RN-16)**
  - Dado uma carteira cujo depósito de 10.000 centavos foi sacado e depois estornado
  - Quando o titular consulta o saldo
  - Então recebe -10.000 centavos: a consulta exibe o saldo negativo tal como o histórico o
    produziu, sem truncar em zero.

- **Borda 4 — carteira inexistente (RN-12)**
  - Dado um identificador de carteira que nunca foi aberto
  - Quando alguém consulta o saldo desse identificador
  - Então a consulta é recusada com o motivo "carteira inexistente"; o sistema não devolve
    saldo 0 nem cria carteira.

- **Borda 5 — carteira de outro titular (RN-14)**
  - Dado um titular e a carteira de outro titular
  - Quando ele consulta o saldo da carteira alheia
  - Então a consulta é recusada.

- **Borda 6 — defasagem após um movimento (ADR-0002)**
  - Dado um depósito de 10.000 centavos acabou de ser aceito
  - Quando o titular consulta o saldo até 2 segundos depois
  - Então o saldo pode ainda não incluir esse depósito; **em até 2 segundos** após o aceite,
    a consulta já reflete o novo valor, e a data/hora do último movimento considerado deixa
    claro a que momento o saldo se refere.

- **Borda 7 — consulta não altera nada**
  - Dado uma carteira com saldo 7.500 centavos
  - Quando o titular consulta o saldo várias vezes seguidas, sem nenhuma nova operação
  - Então recebe 7.500 centavos em todas as consultas e nenhum movimento novo aparece no
    extrato.

- **Borda 8 — valor sempre em centavos inteiros (RN-8)**
  - Dado uma carteira com saldo 7.505 centavos
  - Quando o titular consulta o saldo
  - Então recebe um inteiro de centavos, nunca um valor com fração de centavo.

- **Borda 9 — consulta pelo agente é livre (RN-13)**
  - Dado o titular perguntando ao agente "quanto eu tenho?"
  - Quando o agente consulta o saldo
  - Então ele responde diretamente, sem pedir confirmação — consulta não move dinheiro.

## 5. Requisitos funcionais

- **RF-1:** O sistema DEVE devolver, para uma carteira aberta, o saldo atual igual à soma de
  todos os movimentos registrados nessa carteira (RN-2).
- **RF-2:** O saldo DEVE ser expresso como inteiro de centavos, em BRL (RN-8/RN-9).
- **RF-3:** A resposta DEVE incluir a data/hora do último movimento considerado no saldo; se
  não houver movimento, DEVE indicar a ausência.
- **RF-4:** O sistema DEVE devolver 0 centavos para carteira aberta sem nenhum movimento.
- **RF-5:** O sistema DEVE devolver saldo negativo quando o histórico o produzir por estorno,
  sem truncar em zero (RN-16).
- **RF-6:** O sistema DEVE recusar a consulta de saldo de carteira inexistente (RN-12).
- **RF-7:** O sistema DEVE recusar a consulta de saldo de carteira que não é a do titular
  solicitante (RN-14).
- **RF-8:** A consulta de saldo NÃO DEVE alterar saldo, movimentos ou qualquer estado da
  carteira.
- **RF-9:** O saldo informado DEVE ser reconciliável: recomputá-lo a partir dos movimentos
  exibidos no extrato produz o mesmo valor (RN-6).
- **RF-10:** O saldo DEVE refletir estornos como movimentos próprios, sem remover nem ocultar
  o movimento original (RN-3/RN-4).
- **RF-11:** Um movimento aceito DEVE aparecer no saldo consultado em **até 2 segundos** após
  o aceite.
- **RF-12:** A consulta pelo agente NÃO DEVE exigir confirmação do titular (RN-13).

## 6. Regras de negócio (invariantes do domínio)

- **Dinheiro:** saldo é inteiro de centavos; nunca fração de centavo, nunca ponto flutuante
  (RN-8).
- **Saldo derivado:** o saldo é sempre a soma dos movimentos, nunca um número mantido à parte
  como verdade (RN-2).
- **Moeda:** o saldo é expresso em BRL; não há soma entre moedas (RN-9).
- **Sinal do saldo:** o saldo não fica negativo por saque (RN-10), mas **pode** ficar negativo
  por estorno (RN-16); a consulta mostra o que o histórico produziu.
- **Existência:** só se consulta carteira aberta (RN-12).
- **Ator:** só o titular consulta a própria carteira (RN-14).
- **Consulta é leitura pura:** não produz movimento nem muda o histórico; e é livre para o
  agente (RN-13).
- **Frescor:** a resposta reflete todo movimento aceito há mais de 2 segundos.

## 7. Restrições herdadas (ADR-x / RN-x)

| ID | O que impõe A ESTA feature | Fonte |
|----|----------------------------|-------|
| RN-2 ✓ | O saldo devolvido é derivado dos movimentos registrados; é proibido responder com um número mantido à parte como fonte da verdade. | `docs/business-rules.md` |
| RN-8 ✓ | O saldo é devolvido como inteiro de centavos, sem fração e sem arredondamento. | `docs/business-rules.md` |
| RN-12 ✓ *(herdada de BL-5)* | Consulta a carteira inexistente é recusada, não respondida com saldo 0. | `docs/business-rules.md` |
| RN-13 ✓ | Consulta de saldo é livre para o agente: não exige confirmação explícita, porque não move dinheiro. | `docs/business-rules.md` |
| RN-14 ✓ | Só o titular consulta o saldo da própria carteira. | `docs/business-rules.md` |
| RN-16 ✓ | O saldo pode ser negativo quando um estorno o produzir; a consulta exibe o valor negativo em vez de truncar em zero. | `docs/business-rules.md` |
| ADR-0001 | O histórico é a fonte da verdade; a resposta da consulta é consequência dele e precisa bater com o extrato. | `docs/adr/0001-event-sourcing-no-ledger.md` |
| ADR-0002 | A consulta é servida por um caminho de leitura próprio, separado do que registra os movimentos; consequência observável e aceita: defasagem de **até 2 segundos** entre o aceite e o saldo consultado. | `docs/adr/0002-cqrs.md` |

> Todas as regras acima são ✓ (lei) e viraram RF-x e critério de aceite. Nenhuma regra ⚠
> entra aqui — RN-7 segue ⚠ e não foi adotada (ver seção 9).

## 8. Camada agêntica (linguagem natural)

Como **tool MCP**, esta operação é "consultar saldo": o titular pergunta algo como "quanto eu
tenho na carteira?" e recebe valor, moeda e a data/hora do último movimento considerado.

- **Confirmação:** não exigida — consulta não move dinheiro (RN-13).
- **Autorização na borda da tool (RN-14):** o agente só consulta a carteira do próprio titular
  da conversa.

## 9. Suposições e fora de escopo

- **Decisão registrada (RN-7 ⚠ não adotada na v1):** existe um único conceito de saldo. Não há
  "saldo disponível" versus "saldo pendente", porque não há transação pendente.
- **Suposição:** a meta de 2 segundos é acordo de negócio sobre o frescor da resposta, não
  garantia de leitura imediatamente após a escrita.
- **Fora de escopo agora:** saldo histórico ("quanto eu tinha em janeiro"), saldo projetado,
  saldo consolidado de várias carteiras, saldo em outra moeda.

## 10. Pendências de decisão

Nenhuma. Todas as pendências foram decididas em 2026-09-02.

## 11. Critérios de aceite

- [ ] Consulta em carteira com depósito de 10.000 e saque de 2.500 centavos devolve 7.500
      centavos em BRL.
- [ ] A resposta traz a data/hora do último movimento considerado; em carteira sem movimentos,
      indica a ausência.
- [ ] Consulta em carteira recém-aberta devolve 0 centavos.
- [ ] A soma dos movimentos exibidos no extrato é sempre igual ao saldo devolvido (RN-2/RN-6).
- [ ] Carteira com depósito estornado devolve saldo 0 e mantém os dois movimentos no extrato.
- [ ] Carteira cujo depósito foi sacado e depois estornado devolve saldo negativo, sem truncar
      em zero (RN-16).
- [ ] Consulta de carteira inexistente é recusada com motivo "carteira inexistente" (RN-12).
- [ ] Consulta de carteira de outro titular é recusada (RN-14).
- [ ] Movimento aceito aparece no saldo consultado em até 2 segundos.
- [ ] Consultas repetidas sem nova operação devolvem o mesmo valor e não criam movimento.
- [ ] O saldo devolvido é sempre inteiro de centavos e traz a moeda BRL (RN-8).
- [ ] Consulta pelo agente é respondida sem pedido de confirmação (RN-13).
