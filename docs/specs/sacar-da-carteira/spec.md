# Spec: Sacar dinheiro da carteira

- **ID:** SPEC-2026-09-02-sacar-da-carteira  ·  **Status:** Validada  ·  **Validada em:** 2026-09-02
- **Autor:** business-analyst (FinAgent)  ·  **Data:** 2026-09-02
- **Origem:** BL-6 do `docs/backlog.md`  ·  **Módulo:** wallet

> Esta spec descreve O QUE o sistema deve fazer e POR QUÊ — nunca o COMO técnico.

## 1. Objetivo (por quê)

O saque fecha o par com o depósito: sem ele o dinheiro entra e nunca sai. É também onde a
regra de recusa fica observável — o titular não consegue tirar mais do que tem, e o saque
nunca deixa a carteira negativa (RN-10).

## 2. Atores

- **Titular:** único ator da v1 (RN-14); saca apenas da própria carteira.
- **Sistema FinAgent:** valida saldo e valor, registra o movimento ou recusa com motivo.
- **Contrapartida externa:** o "para onde o dinheiro foi" — o outro lado da transação, fora da
  carteira do titular (ADR-0008).
- **Agente de linguagem natural:** pede o saque em conversa, sempre com confirmação explícita
  do titular (RN-13, ver seção 8).

## 3. Escopo

- **Entra:**
  - registrar um saque de valor positivo, em centavos inteiros, em BRL, numa carteira aberta
    com saldo suficiente;
  - aceitar uma **descrição livre opcional** (texto curto) no saque, exibida no extrato;
  - tratar pedido repetido com a **mesma chave de idempotência** devolvendo o resultado
    original, sem criar movimento novo (RN-15);
  - recusar, com motivo explícito, saque inválido (valor não positivo, saldo insuficiente,
    carteira inexistente, moeda divergente, carteira alheia);
  - deixar o saque aceito visível no histórico e refletido no saldo;
  - garantir que o saque registrado nunca seja alterado nem removido.
- **NÃO entra:**
  - cancelar/corrigir um saque (isso é estorno — `docs/specs/estornar-transacao/spec.md`);
  - **limite máximo por saque ou por período** — a v1 não tem limite além do próprio saldo;
  - **tarifa de saque** — não existe na v1; o saldo cai exatamente no valor sacado;
  - **destino externo informado pelo titular** — a contrapartida externa genérica basta;
  - cheque especial, crédito ou saldo negativo por saque;
  - transferência entre carteiras; conversão de moeda; saque agendado.

## 4. Cenários (comportamento observável)

- **Cenário feliz — saque aceito**
  - Dado uma carteira aberta com saldo 10.000 centavos
  - Quando o titular saca 2.500 centavos em BRL
  - Então o saque é aceito, o saldo passa a ser 7.500 centavos e o extrato passa a exibir um
    movimento de saída de 2.500 centavos.

- **Cenário feliz — saque do saldo inteiro**
  - Dado uma carteira aberta com saldo 10.000 centavos
  - Quando o titular saca exatamente 10.000 centavos
  - Então o saque é aceito e o saldo passa a ser 0 centavos.

- **Cenário feliz — saque com descrição**
  - Dado uma carteira aberta com saldo 10.000 centavos
  - Quando o titular saca 2.500 centavos com a descrição "aluguel"
  - Então o saque é aceito e o extrato exibe esse movimento com a descrição "aluguel".

- **Borda 1 — pedido repetido com a mesma chave de idempotência (RN-15)**
  - Dado um saque de 2.500 centavos já aceito com uma determinada chave de idempotência
  - Quando chega outro pedido de saque com a mesma chave
  - Então o sistema devolve o resultado do saque original, o saldo permanece 7.500 centavos e
    o extrato continua com exatamente um movimento de saída.

- **Borda 2 — saldo insuficiente (RN-10)**
  - Dado uma carteira aberta com saldo 10.000 centavos
  - Quando o titular tenta sacar 10.001 centavos
  - Então o saque é recusado com o motivo "saldo insuficiente"; o saldo continua 10.000
    centavos e nenhum movimento é registrado.

- **Borda 3 — carteira sem saldo (RN-10)**
  - Dado uma carteira recém-aberta, com saldo 0 centavos
  - Quando o titular tenta sacar 1 centavo
  - Então o saque é recusado com o motivo "saldo insuficiente".

- **Borda 4 — saque com saldo negativo por estorno anterior (RN-10 × RN-16)**
  - Dado uma carteira com saldo -10.000 centavos, resultado de um estorno
  - Quando o titular tenta sacar qualquer valor
  - Então o saque é recusado com o motivo "saldo insuficiente": o estorno pode negativar a
    carteira, o saque nunca.

- **Borda 5 — valor zero (RN-11)**
  - Dado uma carteira aberta com saldo 10.000 centavos
  - Quando o titular tenta sacar 0 centavos
  - Então o saque é recusado com o motivo "valor deve ser maior que zero".

- **Borda 6 — valor negativo (RN-11)**
  - Dado uma carteira aberta
  - Quando o titular tenta sacar -500 centavos
  - Então o saque é recusado com o motivo "valor deve ser maior que zero"; em nenhuma hipótese
    um valor negativo vira entrada de dinheiro.

- **Borda 7 — valor com fração de centavo (RN-8)**
  - Dado uma carteira aberta
  - Quando chega um pedido de saque de valor que não é inteiro de centavos
  - Então o saque é recusado, sem arredondar nem truncar.

- **Borda 8 — carteira inexistente (RN-12)**
  - Dado um identificador de carteira que nunca foi aberto
  - Quando alguém tenta sacar dele
  - Então o saque é recusado com o motivo "carteira inexistente".

- **Borda 9 — moeda divergente (RN-9)**
  - Dado uma carteira aberta em BRL
  - Quando o titular tenta sacar um valor expresso em outra moeda
  - Então o saque é recusado com o motivo "moeda divergente"; o sistema não converte.

- **Borda 10 — carteira de outro titular (RN-14)**
  - Dado um titular e a carteira de outro titular
  - Quando ele tenta sacar da carteira alheia
  - Então a operação é recusada.

- **Borda 11 — dois saques simultâneos que juntos excedem o saldo (RN-10)**
  - Dado uma carteira com saldo 10.000 centavos
  - Quando chegam ao mesmo tempo dois pedidos de saque de 8.000 centavos cada, com chaves de
    idempotência diferentes
  - Então no máximo um é aceito, o outro é recusado com "saldo insuficiente" e o saldo final
    nunca fica negativo.

- **Borda 12 — confirmação na conversa (RN-13)**
  - Dado o titular dizendo ao agente "tira 50 reais da minha carteira"
  - Quando o agente entende a intenção
  - Então ele apresenta a operação e só registra o saque após confirmação explícita do
    titular; sem confirmação, nada é registrado.

- **Borda 13 — tentativa de alterar o passado (RN-3)**
  - Dado um saque já registrado
  - Quando se tenta alterar o valor desse movimento ou removê-lo do histórico
  - Então a tentativa é recusada; a única correção possível é um estorno (BL-7).

- **Borda 14 — soma zero da transação (RN-1)**
  - Dado um saque aceito de 2.500 centavos
  - Quando se somam os dois lados da transação (saída da carteira e entrada na contrapartida
    externa)
  - Então o resultado é 0: nenhum centavo é criado nem some.

- **Borda 15 — recusa não deixa rastro**
  - Dado uma carteira com saldo 10.000 centavos
  - Quando um saque é recusado por qualquer motivo
  - Então o saldo continua 10.000 centavos e nenhum movimento novo aparece no extrato.

## 5. Requisitos funcionais

- **RF-1:** O sistema DEVE registrar um saque de valor maior que zero, em centavos inteiros,
  em BRL, numa carteira aberta com saldo suficiente, reduzindo o saldo exatamente no valor
  sacado.
- **RF-2:** O sistema DEVE recusar saque cujo valor seja maior que o saldo atual, informando o
  motivo "saldo insuficiente" (RN-10).
- **RF-3:** Nenhum saque DEVE deixar o saldo negativo, em nenhuma sequência ou concorrência de
  operações (RN-10).
- **RF-4:** O sistema DEVE aceitar saque de valor exatamente igual ao saldo, deixando o saldo
  em 0 centavos.
- **RF-5:** O sistema DEVE aceitar uma descrição livre opcional (texto curto) no saque e
  exibi-la no extrato.
- **RF-6:** O sistema DEVE exigir chave de idempotência no saque e, para pedido repetido com a
  mesma chave, DEVE devolver o resultado do saque original sem criar movimento novo (RN-15).
- **RF-7:** O sistema DEVE recusar saque com valor menor ou igual a zero (RN-11).
- **RF-8:** O sistema DEVE recusar saque cujo valor não seja inteiro de centavos, sem
  arredondar nem truncar (RN-8).
- **RF-9:** O sistema DEVE recusar saque em carteira inexistente (RN-12).
- **RF-10:** O sistema DEVE recusar saque expresso em moeda diferente de BRL, sem converter
  (RN-9).
- **RF-11:** O sistema DEVE recusar saque em carteira que não é a do titular solicitante
  (RN-14).
- **RF-12:** Todo saque aceito DEVE produzir exatamente um registro correspondente no
  histórico da carteira (RN-5).
- **RF-13:** Todo saque aceito DEVE ter contrapartida de mesmo valor fora da carteira, de modo
  que a transação some zero (RN-1).
- **RF-14:** Um saque registrado NÃO DEVE ser alterado nem removido; toda tentativa é recusada
  (RN-3).
- **RF-15:** Um saque recusado NÃO DEVE alterar o saldo nem acrescentar movimento ao extrato.
- **RF-16:** O saque aceito DEVE ser identificável de forma única no extrato, para poder ser
  referenciado por um estorno (BL-7).
- **RF-17:** Quando pedido pelo agente, o saque só DEVE ser registrado após confirmação
  explícita do titular na conversa (RN-13).
- **RF-18:** O saque NÃO DEVE ter limite por valor ou por período na v1, nem tarifa: o saldo
  cai exatamente no valor sacado.

## 6. Regras de negócio (invariantes do domínio)

- **Dinheiro:** todo valor é inteiro de centavos; proibido ponto flutuante e fração de centavo
  (RN-8).
- **Valor:** saque exige valor estritamente maior que zero (RN-11); sem teto além do saldo.
- **Saldo:** o saque nunca deixa a carteira negativa — saque maior que o saldo é recusado
  (RN-10). Cheque especial não existe. A **única** exceção decidida ao saldo negativo é o
  estorno (RN-16), que é outra capacidade.
- **Moeda:** BRL; sem conversão (RN-9).
- **Existência:** só se saca de carteira já aberta (RN-12).
- **Ator:** só o titular saca da própria carteira (RN-14).
- **Idempotência:** mesma chave = mesmo resultado, nunca um segundo lançamento (RN-15).
- **Completude:** nenhum dinheiro sai sem registro correspondente (RN-5).
- **Imutabilidade:** saque registrado não muda e não é apagado (RN-3); correção só por estorno
  (RN-4).
- **Soma zero:** toda transação tem dois lados de mesmo valor e soma zero (RN-1).

## 7. Restrições herdadas (ADR-x / RN-x)

| ID | O que impõe A ESTA feature | Fonte |
|----|----------------------------|-------|
| RN-10 ✓ | Saque maior que o saldo é recusado; o saque nunca deixa a carteira negativa, nem em concorrência. É o saque que RN-10 protege — ver RN-16 para a exceção do estorno. | `docs/business-rules.md` |
| RN-11 ✓ | Saque só é aceito com valor estritamente maior que zero. | `docs/business-rules.md` |
| RN-5 ✓ | Todo saque aceito gera registro correspondente: dinheiro não sai sem registro. | `docs/business-rules.md` |
| RN-3 ✓ | Saque registrado é definitivo: não se altera nem se apaga; correção só por estorno. | `docs/business-rules.md` |
| RN-1 ✓ | O saque é transação de dois lados que somam zero: sai da carteira o mesmo valor que entra na contrapartida externa. | `docs/business-rules.md` |
| RN-8 ✓ | O valor sacado é inteiro de centavos; sem fração e sem arredondamento. | `docs/business-rules.md` |
| RN-9 ✓ | Saque em moeda diferente de BRL é recusado; sem conversão. | `docs/business-rules.md` |
| RN-12 ✓ *(herdada de BL-5)* | Só se saca de carteira já aberta. | `docs/business-rules.md` |
| RN-13 ✓ | Saque move dinheiro: pedido pelo agente exige confirmação explícita do titular antes de ser efetivado. | `docs/business-rules.md` |
| RN-14 ✓ | O titular saca apenas da própria carteira; não há saque por outro ator. | `docs/business-rules.md` |
| RN-15 ✓ | O saque carrega chave de idempotência; pedido repetido com a mesma chave devolve o resultado original e não cria movimento novo. | `docs/business-rules.md` |
| RN-16 ✓ *(fronteira)* | A permissão de saldo negativo vale para o estorno, NÃO para o saque: uma carteira já negativa por estorno continua recusando saque por RN-10. | `docs/business-rules.md` |
| ADR-0001 | O saldo verificado antes de aceitar/recusar é derivado do histórico, e o saque é fato acrescentado a ele — nunca sobrescrita de saldo. | `docs/adr/0001-event-sourcing-no-ledger.md` |
| ADR-0008 | O saque é transação balanceada entre a carteira do titular e a contrapartida externa; a validação de saldo insuficiente acontece antes de a transação ser registrada, e ela é registrada inteira ou não é registrada. | `docs/adr/0008-partida-dobrada-no-ledger.md` |

> Todas as regras acima são ✓ (lei) e viraram RF-x e critério de aceite. Nenhuma regra ⚠
> entra aqui — RN-7 segue ⚠ e não foi adotada na v1 (ver seção 9).

## 8. Camada agêntica (linguagem natural)

Como **tool MCP**, esta operação é "sacar da carteira": o titular diz algo como "tira 50 reais
da minha carteira".

- **Confirmação obrigatória (RN-13):** por mover dinheiro, o agente apresenta a operação
  (valor, moeda, carteira) e só a efetiva após confirmação explícita do titular. Sem
  confirmação, nada é registrado.
- **Autorização na borda da tool (RN-14):** o agente só saca da carteira do próprio titular da
  conversa.
- **Repetição (RN-15):** reexecução do mesmo pedido, com a mesma chave de idempotência,
  devolve o saque original — retry de conversa não saca duas vezes.
- **Recusa:** saldo insuficiente é devolvido ao titular como motivo explícito, não como erro
  genérico.

## 9. Suposições e fora de escopo

- **Decisão registrada (RN-7 ⚠ não adotada na v1):** o saque aceito já nasce definitivo; não
  há reserva de valor nem estado "pendente".
- **Suposição:** o destino do dinheiro é a contrapartida externa genérica do sistema; o titular
  não informa conta de destino.
- **Suposição:** a descrição livre é o mesmo campo opcional do depósito — texto curto, sem
  efeito no cálculo do saldo, exibido no extrato.
- **Fora de escopo agora:** cheque especial, limite de crédito, limites por valor/período,
  tarifas, transferência entre carteiras, saque agendado.

## 10. Pendências de decisão

Nenhuma. Todas as pendências foram decididas em 2026-09-02.

## 11. Critérios de aceite

- [ ] Saque de 2.500 centavos em carteira com 10.000 centavos é aceito e deixa saldo 7.500.
- [ ] Saque de valor exatamente igual ao saldo é aceito e deixa saldo 0.
- [ ] Saque com descrição exibe a descrição no extrato.
- [ ] Pedido repetido com a mesma chave de idempotência devolve o resultado original e não
      cria movimento novo (RN-15).
- [ ] Saque de valor maior que o saldo é recusado com motivo "saldo insuficiente" e não altera
      nada (RN-10).
- [ ] Saque em carteira com saldo 0 é recusado (RN-10).
- [ ] Saque em carteira com saldo negativo por estorno é recusado com "saldo insuficiente"
      (RN-10 × RN-16).
- [ ] Saque de 0 ou de valor negativo é recusado com motivo "valor deve ser maior que zero"
      (RN-11).
- [ ] Saque com valor que não é inteiro de centavos é recusado, sem arredondamento (RN-8).
- [ ] Saque em carteira inexistente é recusado com motivo "carteira inexistente" (RN-12).
- [ ] Saque em moeda diferente de BRL é recusado com motivo "moeda divergente" (RN-9).
- [ ] Saque em carteira de outro titular é recusado (RN-14).
- [ ] Dois saques concorrentes que juntos excedem o saldo resultam em no máximo um aceito, e o
      saldo final nunca é negativo (RN-10).
- [ ] Saque solicitado pelo agente sem confirmação explícita não registra nada; com
      confirmação, registra (RN-13).
- [ ] Toda tentativa de alterar ou remover um saque registrado é recusada (RN-3).
- [ ] Para todo saque aceito, a soma dos dois lados da transação é 0 (RN-1).
- [ ] Após qualquer saque recusado, saldo e extrato permanecem exatamente como antes.
