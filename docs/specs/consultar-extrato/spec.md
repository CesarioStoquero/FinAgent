# Spec: Consultar o extrato da carteira

- **ID:** SPEC-2026-09-02-consultar-extrato  ·  **Status:** Validada  ·  **Validada em:** 2026-09-02
- **Autor:** business-analyst (FinAgent)  ·  **Data:** 2026-09-02
- **Origem:** BL-3 do `docs/backlog.md`  ·  **Módulo:** wallet

> Esta spec descreve O QUE o sistema deve fazer e POR QUÊ — nunca o COMO técnico.

## 1. Objetivo (por quê)

O extrato é a prova de que o saldo é explicável: o titular vê a sequência de movimentos que
leva do zero até o valor atual, sem lacunas (RN-6). Sem esta capacidade, a auditabilidade
prometida pela ADR-0001 existe no papel mas ninguém consegue observar.

## 2. Atores

- **Titular:** único ator da v1 (RN-14); consulta apenas o extrato da própria carteira.
- **Sistema FinAgent:** devolve a sequência de movimentos da carteira.
- **Agente de linguagem natural:** consulta livremente, sem confirmação (RN-13, ver seção 8).

## 3. Escopo

- **Entra:**
  - consultar os movimentos de uma carteira aberta, em ordem cronológica crescente, sem
    lacunas;
  - **paginação desde a v1:** período padrão **últimos 30 dias**, página de **50 movimentos**;
  - exibir, para cada movimento: data/hora, tipo (depósito, saque, estorno), valor em centavos,
    moeda, **descrição** (o motivo, no caso do estorno), **saldo acumulado após aquele
    movimento** e um identificador único da transação;
  - exibir o movimento original e o respectivo estorno, ambos visíveis e encadeados;
  - devolver a data/hora do último movimento considerado, como na consulta de saldo;
  - recusar, com motivo explícito, consulta a carteira inexistente ou de outro titular.
- **NÃO entra:**
  - alterar, ocultar ou remover qualquer movimento — o lançamento nunca é apagado;
  - o saldo como capacidade própria (`docs/specs/consultar-saldo/spec.md`);
  - exportar arquivo (PDF/CSV), enviar por e-mail, extrato consolidado de várias carteiras;
  - filtros por tipo de movimento; conversão de moeda.

## 4. Cenários (comportamento observável)

- **Cenário feliz — sequência completa**
  - Dado uma carteira aberta com um depósito de 10.000 centavos e depois um saque de 2.500
    centavos, ambos nos últimos 30 dias
  - Quando o titular consulta o extrato sem informar período
  - Então recebe os dois movimentos, do mais antigo ao mais recente, cada um com data/hora,
    tipo, valor em centavos, moeda BRL, descrição (quando houver) e saldo acumulado — 10.000
    na linha do depósito e 7.500 na linha do saque.

- **Cenário feliz — o extrato explica o saldo (RN-6)**
  - Dado uma carteira com uma sequência qualquer de movimentos
  - Quando o titular soma todos os movimentos do extrato, do primeiro ao último
  - Então o resultado é exatamente o saldo informado na consulta de saldo, e é igual ao saldo
    acumulado exibido na última linha — sem diferença de um centavo e sem movimento faltando.

- **Cenário feliz — carteira sem movimentos**
  - Dado uma carteira recém-aberta
  - Quando o titular consulta o extrato
  - Então recebe uma lista vazia de movimentos (e não um erro).

- **Borda 1 — período padrão de 30 dias**
  - Dado uma carteira com um movimento de 60 dias atrás e um movimento de ontem
  - Quando o titular consulta o extrato sem informar período
  - Então recebe apenas o movimento de ontem; ao informar explicitamente um período que cubra
    os 60 dias, recebe os dois.

- **Borda 2 — paginação de 50 movimentos**
  - Dado uma carteira com 120 movimentos dentro do período consultado
  - Quando o titular consulta o extrato
  - Então recebe a primeira página com 50 movimentos, em ordem cronológica crescente, e a
    indicação de que há mais páginas; percorrendo todas as páginas ele vê os 120 movimentos,
    cada um exatamente uma vez, sem lacuna e sem repetição.

- **Borda 3 — estorno aparece encadeado e o original permanece (RN-3/RN-4)**
  - Dado uma carteira com um depósito de 10.000 centavos que foi estornado
  - Quando o titular consulta o extrato
  - Então vê os dois movimentos: o depósito original, intacto, e o estorno, identificado como
    estorno, exibindo o motivo obrigatório e apontando para a transação que ele anula.

- **Borda 4 — saldo acumulado negativo por estorno (RN-16)**
  - Dado uma carteira em que um depósito de 10.000 centavos foi sacado e depois estornado
  - Quando o titular consulta o extrato
  - Então a linha do estorno exibe saldo acumulado -10.000 centavos, sem truncar em zero.

- **Borda 5 — carteira inexistente (RN-12)**
  - Dado um identificador de carteira que nunca foi aberto
  - Quando alguém consulta o extrato desse identificador
  - Então a consulta é recusada com o motivo "carteira inexistente"; o sistema não devolve
    lista vazia.

- **Borda 6 — carteira de outro titular (RN-14)**
  - Dado um titular e a carteira de outro titular
  - Quando ele consulta o extrato da carteira alheia
  - Então a consulta é recusada.

- **Borda 7 — retenção indefinida**
  - Dado uma carteira com movimentos de vários anos atrás
  - Quando o titular consulta o extrato informando um período que os cubra
  - Então esses movimentos continuam disponíveis: lançamento nunca é apagado nem expira.

- **Borda 8 — defasagem após um movimento (ADR-0002/ADR-0005)**
  - Dado um depósito acabou de ser aceito
  - Quando o titular consulta o extrato até 2 segundos depois
  - Então o movimento pode ainda não aparecer; **em até 2 segundos** após o aceite ele já
    aparece, e a data/hora do último movimento considerado deixa claro a que momento o extrato
    se refere.

- **Borda 9 — consulta não altera nada**
  - Dado uma carteira com movimentos
  - Quando o titular consulta o extrato várias vezes seguidas, sem nenhuma nova operação
  - Então recebe exatamente a mesma sequência em todas as consultas e nenhum movimento novo
    aparece.

- **Borda 10 — movimento recusado não aparece (RN-5)**
  - Dado uma carteira cujo saque foi recusado por saldo insuficiente
  - Quando o titular consulta o extrato
  - Então a tentativa recusada não aparece como movimento: só o que efetivamente moveu
    dinheiro está no extrato.

- **Borda 11 — pedido idempotente não duplica linha (RN-15)**
  - Dado um depósito repetido com a mesma chave de idempotência
  - Quando o titular consulta o extrato
  - Então vê apenas um movimento correspondente àquele depósito.

- **Borda 12 — tentativa de alterar o passado (RN-3)**
  - Dado uma carteira com movimentos registrados
  - Quando se tenta alterar ou remover um movimento exibido no extrato
  - Então a tentativa é recusada e o extrato permanece idêntico ao consultado antes.

- **Borda 13 — consulta pelo agente é livre (RN-13)**
  - Dado o titular pedindo ao agente "me mostra meus últimos movimentos"
  - Quando o agente consulta o extrato
  - Então ele responde diretamente, sem pedir confirmação — consulta não move dinheiro.

## 5. Requisitos funcionais

- **RF-1:** O sistema DEVE devolver, para uma carteira aberta, os movimentos daquela carteira
  em ordem cronológica crescente (do mais antigo ao mais recente).
- **RF-2:** A sequência devolvida NÃO DEVE ter lacunas: todo movimento que afetou o saldo
  aparece exatamente uma vez dentro do período consultado (RN-6).
- **RF-3:** O sistema DEVE aplicar, quando o titular não informar período, o **período padrão
  de últimos 30 dias**, e DEVE permitir informar outro período explicitamente.
- **RF-4:** O sistema DEVE paginar o resultado em páginas de **50 movimentos**, indicando se
  há mais páginas; percorrer todas as páginas DEVE devolver cada movimento exatamente uma vez.
- **RF-5:** Cada movimento DEVE exibir: data/hora, tipo (depósito, saque, estorno), valor em
  centavos inteiros, moeda BRL, descrição (motivo, no caso do estorno), saldo acumulado após
  aquele movimento e identificador único da transação.
- **RF-6:** A soma dos valores dos movimentos DEVE ser igual ao saldo devolvido pela consulta
  de saldo, e igual ao saldo acumulado da última linha do histórico (RN-2/RN-6).
- **RF-7:** Um movimento estornado DEVE continuar visível, e o estorno DEVE aparecer como
  movimento próprio, referenciando a transação estornada e exibindo o motivo (RN-3/RN-4).
- **RF-8:** O saldo acumulado DEVE ser exibido como valor negativo quando o histórico assim o
  produzir por estorno, sem truncar em zero (RN-16).
- **RF-9:** O sistema DEVE devolver lista vazia (não erro) para carteira aberta sem movimentos.
- **RF-10:** O sistema DEVE recusar a consulta de extrato de carteira inexistente (RN-12).
- **RF-11:** O sistema DEVE recusar a consulta de extrato de carteira que não é a do titular
  solicitante (RN-14).
- **RF-12:** O sistema NÃO DEVE apagar, expirar nem ocultar movimento algum: a retenção é
  indefinida (RN-3).
- **RF-13:** A consulta de extrato NÃO DEVE alterar saldo, movimentos ou qualquer estado da
  carteira.
- **RF-14:** Operações recusadas NÃO DEVEM aparecer no extrato como movimento (RN-5).
- **RF-15:** Um movimento aceito DEVE aparecer no extrato em **até 2 segundos** após o aceite,
  e a resposta DEVE trazer a data/hora do último movimento considerado.
- **RF-16:** Consultas repetidas sem nova operação DEVEM devolver exatamente a mesma sequência,
  na mesma ordem.
- **RF-17:** A consulta pelo agente NÃO DEVE exigir confirmação do titular (RN-13).

## 6. Regras de negócio (invariantes do domínio)

- **Dinheiro:** todo valor exibido é inteiro de centavos (RN-8).
- **Rastreabilidade:** cada centavo do saldo é explicável percorrendo o extrato, sem lacunas
  (RN-6); o saldo acumulado por linha torna isso verificável linha a linha.
- **Imutabilidade:** movimento exibido nunca muda de valor nem desaparece (RN-3); correção
  aparece como estorno acrescentado, nunca como edição (RN-4).
- **Retenção:** indefinida — lançamento nunca é apagado.
- **Completude:** só aparece no extrato o que efetivamente moveu dinheiro; e tudo que moveu
  dinheiro aparece (RN-5).
- **Moeda:** todos os movimentos estão em BRL; o extrato não mistura moedas nem converte
  (RN-9).
- **Sinal do saldo acumulado:** pode ser negativo por estorno (RN-16); nunca por saque (RN-10).
- **Existência:** só se consulta extrato de carteira aberta (RN-12).
- **Ator:** só o titular consulta o extrato da própria carteira (RN-14).
- **Consulta é leitura pura:** não produz movimento; é livre para o agente (RN-13).
- **Frescor:** a resposta reflete todo movimento aceito há mais de 2 segundos.

## 7. Restrições herdadas (ADR-x / RN-x)

| ID | O que impõe A ESTA feature | Fonte |
|----|----------------------------|-------|
| RN-6 ✓ | O extrato precisa permitir explicar cada centavo do saldo percorrendo os movimentos, sem lacunas — é a razão de existir desta feature, reforçada pelo saldo acumulado por linha. | `docs/business-rules.md` |
| RN-3 ✓ | Movimento exibido nunca muda nem é removido; a retenção é indefinida. | `docs/business-rules.md` |
| RN-4 ✓ | O estorno aparece como movimento próprio encadeado ao original, que permanece visível. | `docs/business-rules.md` |
| RN-5 ✓ | Só o que moveu dinheiro entra no extrato; operação recusada não vira linha. | `docs/business-rules.md` |
| RN-8 ✓ | Todo valor exibido (movimento e saldo acumulado) é inteiro de centavos. | `docs/business-rules.md` |
| RN-12 ✓ *(herdada de BL-5)* | Consulta a carteira inexistente é recusada, não respondida com lista vazia. | `docs/business-rules.md` |
| RN-13 ✓ | Consulta de extrato é livre para o agente: não exige confirmação, porque não move dinheiro. | `docs/business-rules.md` |
| RN-14 ✓ | Só o titular consulta o extrato da própria carteira. | `docs/business-rules.md` |
| RN-15 ✓ | Pedido idempotente repetido não gera segunda linha no extrato. | `docs/business-rules.md` |
| RN-16 ✓ | O saldo acumulado pode ficar negativo por estorno e é exibido como tal. | `docs/business-rules.md` |
| ADR-0001 | O extrato é a face visível do histórico, que é a fonte da verdade; não é montado a partir de um saldo guardado à parte. | `docs/adr/0001-event-sourcing-no-ledger.md` |
| ADR-0002 | A consulta é servida por um caminho de leitura próprio, separado do que registra os movimentos; defasagem aceita de **até 2 segundos** entre o aceite e a aparição no extrato. | `docs/adr/0002-cqrs.md` |
| ADR-0005 | O extrato é desenhado para leitura por período e paginação — daí o período padrão de 30 dias e a página de 50 movimentos. | `docs/adr/0005-mongodb-read-model.md` |

> Todas as regras acima são ✓ (lei) e viraram RF-x e critério de aceite. Nenhuma regra ⚠
> entra aqui — RN-7 segue ⚠ e não foi adotada; as LGPD-x não se aplicam porque a v1 não
> coleta PII (ver seção 9).

## 8. Camada agêntica (linguagem natural)

Como **tool MCP**, esta operação é "consultar extrato": o titular pede algo como "me mostra os
últimos movimentos da minha carteira" e recebe a sequência paginada.

- **Confirmação:** não exigida — consulta não move dinheiro (RN-13).
- **Autorização na borda da tool (RN-14):** o agente só lê o extrato da carteira do próprio
  titular da conversa.
- **Recorte:** sem período informado, o agente devolve os últimos 30 dias, em páginas de 50
  movimentos, e oferece a página seguinte quando houver.

## 9. Suposições e fora de escopo

- **Suposição:** a ordenação é cronológica crescente (do primeiro ao último movimento), como
  descrito no item BL-3.
- **Suposição:** o extrato lista os movimentos financeiros; a abertura da carteira não é
  movimento financeiro e não entra na soma.
- **Suposição:** a descrição exibida é a informada no depósito/saque (opcional) e o motivo do
  estorno (obrigatório).
- **Decisão registrada (RN-7 ⚠ não adotada na v1):** não há movimento "pendente" a exibir; todo
  movimento no extrato é definitivo.
- **Sem PII na v1:** nenhum movimento carrega dado pessoal; a descrição é texto livre do
  próprio titular. Se a v1 passar a coletar PII, as LGPD-1..9 (⚠) precisam ser confirmadas
  antes — inclusive por causa da retenção indefinida.
- **Fora de escopo agora:** exportação de arquivo, extrato de período fechado ("fatura"),
  filtros por tipo, extrato consolidado, ordenação decrescente.

## 10. Pendências de decisão

Nenhuma. Todas as pendências foram decididas em 2026-09-02.

## 11. Critérios de aceite

- [ ] Extrato de carteira com um depósito e um saque devolve os dois movimentos em ordem
      cronológica crescente, com saldo acumulado 10.000 e 7.500 respectivamente.
- [ ] Consulta sem período informado devolve apenas os movimentos dos últimos 30 dias; com
      período informado, devolve os do período pedido.
- [ ] Carteira com 120 movimentos no período devolve páginas de 50; percorrer as páginas
      mostra os 120 movimentos, cada um uma vez, sem lacuna nem repetição.
- [ ] A soma dos valores dos movimentos é igual ao saldo consultado e ao saldo acumulado da
      última linha (RN-6/RN-2).
- [ ] Extrato de carteira recém-aberta devolve lista vazia, não erro.
- [ ] Depósito estornado aparece junto com o estorno, ambos visíveis, com o estorno exibindo o
      motivo e referenciando a transação original (RN-3/RN-4).
- [ ] Saldo acumulado negativo por estorno é exibido como negativo (RN-16).
- [ ] Extrato de carteira inexistente é recusado com motivo "carteira inexistente" (RN-12).
- [ ] Extrato de carteira de outro titular é recusado (RN-14).
- [ ] Movimentos antigos continuam disponíveis quando o período os cobre (retenção indefinida).
- [ ] Movimento aceito aparece no extrato em até 2 segundos, e a resposta traz a data/hora do
      último movimento considerado.
- [ ] Consultas repetidas sem nova operação devolvem a mesma sequência, na mesma ordem.
- [ ] Operação recusada (ex.: saque por saldo insuficiente) não aparece no extrato.
- [ ] Depósito repetido com a mesma chave de idempotência gera uma única linha (RN-15).
- [ ] Toda tentativa de alterar ou remover um movimento é recusada e o extrato permanece
      idêntico (RN-3).
- [ ] Cada movimento traz data/hora, tipo, valor inteiro em centavos, moeda BRL, descrição,
      saldo acumulado e identificador único da transação.
- [ ] Consulta pelo agente é respondida sem pedido de confirmação (RN-13).
