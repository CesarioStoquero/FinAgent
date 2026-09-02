# Spec: Abrir uma carteira

- **ID:** SPEC-2026-09-02-abrir-carteira  ·  **Status:** Validada  ·  **Validada em:** 2026-09-02
- **Autor:** business-analyst (FinAgent)  ·  **Data:** 2026-09-02
- **Origem:** BL-5 do `docs/backlog.md`  ·  **Módulo:** wallet

> Esta spec descreve O QUE o sistema deve fazer e POR QUÊ — nunca o COMO técnico.

## 1. Objetivo (por quê)

O titular precisa de um lugar próprio e identificado onde o dinheiro dele repousa. Sem essa
carteira aberta, nenhuma operação do produto existe: não há onde depositar, de onde sacar,
nem o que consultar (RN-12). Esta é a capacidade-raiz — depósito (BL-1), saque (BL-6),
consulta (BL-2/BL-3) e estorno (BL-7) todos dependem dela.

## 2. Atores

- **Titular:** único ator da v1 (RN-14); opera exclusivamente a própria carteira. Na v1 ele é
  um **identificador opaco** — o sistema não coleta nome, CPF, e-mail nem telefone.
- **Sistema FinAgent:** registra a abertura e recusa qualquer operação sobre carteira
  inexistente.
- **Agente de linguagem natural:** pode pedir a abertura em conversa (ver seção 8).

## 3. Escopo

- **Entra:**
  - abrir uma carteira para um titular, com identificador único e estável;
  - a carteira nasce em **BRL**, sem nenhum movimento e, portanto, com saldo 0 centavos;
  - a carteira nasce apta a receber depósito, saque, consulta e estorno;
  - um pedido de abertura para um titular que já tem carteira **devolve a carteira existente**;
  - recusar, com motivo explícito, qualquer operação sobre carteira que não foi aberta.
- **NÃO entra:**
  - **encerrar, bloquear ou suspender carteira** — fora da v1;
  - **mais de uma carteira por titular** — a v1 tem no máximo uma;
  - **escolher a moeda** — BRL é fixa na v1; não há multi-moeda nem conversão;
  - **qualquer dado pessoal do titular** (nome, CPF, e-mail, telefone) — a v1 não coleta PII;
  - depósito, saque, extrato e estorno (specs próprias).

## 4. Cenários (comportamento observável)

- **Cenário feliz — abertura**
  - Dado um titular que ainda não tem carteira
  - Quando ele solicita a abertura de uma carteira
  - Então a carteira passa a existir, com identificador único, moeda BRL, nenhum movimento
    registrado e saldo 0 centavos.

- **Cenário feliz — carteira aberta aceita operação**
  - Dado uma carteira recém-aberta
  - Quando o titular deposita um valor válido nela
  - Então a operação é aceita (comportamento em `docs/specs/depositar-na-carteira/spec.md`).

- **Borda 1 — segunda abertura para o mesmo titular**
  - Dado um titular que já tem uma carteira, com saldo 10.000 centavos
  - Quando ele solicita a abertura de outra carteira
  - Então o sistema devolve a **carteira existente** (mesmo identificador, saldo 10.000
    centavos) e nenhuma segunda carteira passa a existir.

- **Borda 2 — operar carteira inexistente (RN-12)**
  - Dado um identificador de carteira que nunca foi aberto
  - Quando alguém tenta depositar, sacar, estornar, consultar saldo ou consultar extrato
    nesse identificador
  - Então a operação é recusada com o motivo "carteira inexistente" e nada é registrado.

- **Borda 3 — moeda da operação diferente de BRL (RN-9)**
  - Dado uma carteira aberta (em BRL)
  - Quando chega uma operação expressa em outra moeda
  - Então a operação é recusada com o motivo "moeda divergente"; o sistema não converte
    valores.

- **Borda 4 — a abertura é fato definitivo (RN-3)**
  - Dado uma carteira já aberta
  - Quando se tenta apagar ou reescrever o registro da abertura
  - Então a tentativa é recusada; o histórico da carteira começa na abertura e não é
    reescrito.

- **Borda 5 — titular opera apenas a própria carteira (RN-14)**
  - Dado dois titulares, cada um com sua carteira
  - Quando um deles tenta operar ou consultar a carteira do outro
  - Então a operação é recusada; a v1 não tem ator que opere carteira alheia.

## 5. Requisitos funcionais

- **RF-1:** O sistema DEVE abrir uma carteira para um titular, atribuindo a ela um
  identificador único e estável ao longo de toda a vida da carteira.
- **RF-2:** A carteira recém-aberta DEVE apresentar saldo igual a 0 centavos e nenhum
  movimento no extrato.
- **RF-3:** A carteira DEVE ser aberta em **BRL**, sem que a moeda seja informada no pedido;
  a moeda NÃO DEVE mudar ao longo da vida da carteira (RN-9).
- **RF-4:** O sistema DEVE manter no máximo **uma carteira por titular**; um pedido de
  abertura para titular que já tem carteira DEVE devolver a carteira existente, sem criar
  outra e sem alterar saldo ou extrato (RN-15).
- **RF-5:** O sistema DEVE recusar toda operação (depósito, saque, estorno, consulta de saldo,
  consulta de extrato) dirigida a carteira que não foi aberta, informando o motivo "carteira
  inexistente" (RN-12).
- **RF-6:** Toda operação sobre a carteira DEVE ser expressa em BRL; valor em outra moeda é
  recusado, sem conversão (RN-9).
- **RF-7:** A abertura DEVE ficar registrada no histórico da carteira como o primeiro fato da
  sua linha do tempo, e esse registro NÃO DEVE ser alterado nem removido (RN-3).
- **RF-8:** O sistema DEVE permitir que apenas o titular opere e consulte a própria carteira
  (RN-14).
- **RF-9:** A carteira aberta DEVE ser imediatamente apta a receber depósito e a ser
  consultada.
- **RF-10:** O sistema NÃO DEVE coletar nem armazenar dado pessoal do titular (nome, CPF,
  e-mail, telefone); o titular é referenciado por identificador opaco.
- **RF-11:** Uma abertura recusada NÃO DEVE deixar carteira parcialmente criada nem
  identificador reservado.

## 6. Regras de negócio (invariantes do domínio)

- **Dinheiro:** todo valor é inteiro de centavos; o saldo inicial é 0 centavos. Proibido
  fração de centavo e ponto flutuante (RN-8).
- **Moeda:** BRL, fixa na abertura e imutável; operação em moeda diferente é recusada, nunca
  convertida (RN-9).
- **Uma carteira por titular:** o titular tem no máximo uma carteira na v1; pedir abertura de
  novo devolve a mesma (RN-15).
- **Saldo:** nunca é um número guardado à parte — é sempre a soma dos movimentos; a carteira
  recém-aberta tem saldo 0 por ausência de movimentos (RN-2).
- **Existência:** operar exige carteira previamente aberta (RN-12).
- **Ator:** só o titular opera a própria carteira (RN-14).
- **Contrapartida:** todo movimento futuro terá outro lado de mesmo valor fora da carteira,
  somando zero (RN-1). A abertura em si não move dinheiro.
- **Sem PII:** o titular é um identificador opaco; nenhum dado pessoal entra no sistema.

## 7. Restrições herdadas (ADR-x / RN-x)

| ID | O que impõe A ESTA feature | Fonte |
|----|----------------------------|-------|
| RN-12 ✓ | Só se opera carteira já aberta: esta feature cria essa pré-condição e obriga a recusa explícita em carteira inexistente. | `docs/business-rules.md` |
| RN-9 ✓ | A carteira tem uma única moeda (BRL na v1) e toda operação nela usa essa moeda; moeda divergente é recusada, sem conversão. | `docs/business-rules.md` |
| RN-14 ✓ | O único ator é o titular, na própria carteira: a abertura é do próprio titular e ninguém opera carteira alheia. | `docs/business-rules.md` |
| RN-15 ✓ | Pedido repetido não duplica: a segunda abertura do mesmo titular devolve a carteira existente, sem criar lançamento nem carteira nova. | `docs/business-rules.md` |
| RN-13 ✓ | A abertura não move dinheiro, logo não exige confirmação explícita na conversa; ainda assim o agente só a faz para o próprio titular. | `docs/business-rules.md` |
| RN-3 ✓ *(invariante geral do módulo)* | O registro da abertura é definitivo: não se altera nem se apaga. | `docs/business-rules.md` |
| ADR-0001 | O histórico da carteira é a fonte da verdade: a abertura é um fato registrado e o saldo é derivado dos movimentos, nunca um campo mantido à parte. | `docs/adr/0001-event-sourcing-no-ledger.md` |
| ADR-0008 | A carteira do titular é uma conta do tipo `Customer` no ledger de partida dobrada; existe uma contrapartida externa do sistema, e toda movimentação futura terá dois lados que somam zero. | `docs/adr/0008-partida-dobrada-no-ledger.md` |

> Todas as regras acima são ✓ (lei) e viraram RF-x e critério de aceite. Nenhuma regra ⚠
> entra aqui: RN-7 não foi adotada na v1 (ver seção 9) e as LGPD-x não se aplicam porque a
> v1 não coleta PII.

## 8. Camada agêntica (linguagem natural)

Como **tool MCP**, esta operação é "abrir carteira": o titular diz algo como "me abre uma
carteira" e recebe a confirmação com o identificador.

- **Confirmação:** não exigida — a abertura não move dinheiro (RN-13 reserva a confirmação
  explícita para depósito e saque).
- **Autorização na borda da tool:** o agente só abre carteira para o próprio titular da
  conversa; não existe abertura em nome de terceiro (RN-14).
- **Repetição:** se o titular pedir de novo, o agente devolve a carteira existente em vez de
  criar outra (RN-15).

## 9. Suposições e fora de escopo

- **Suposição:** a abertura não movimenta dinheiro; o primeiro valor entra pelo depósito (BL-1).
- **Decisão registrada (RN-7 ⚠ não adotada na v1):** a transação nasce postada; não existe
  estado "pendente". Se um dia a RN-7 virar lei, esta spec precisa ser revisitada.
- **Sem PII na v1:** o titular é um identificador opaco. **Se isso mudar** (passar a coletar
  nome, CPF, e-mail ou telefone), as regras **LGPD-1..9 — todas ⚠ hoje** — precisam ser
  confirmadas e viradas ✓ **antes** de a carteira guardar qualquer dado pessoal.
- **Fora de escopo agora:** encerramento/bloqueio de carteira, segunda carteira por titular,
  carteira compartilhada, multi-moeda com conversão, tarifas de abertura.

## 10. Pendências de decisão

Nenhuma. Todas as pendências foram decididas em 2026-09-02.

## 11. Critérios de aceite

- [ ] Abrir uma carteira devolve um identificador único, moeda BRL, saldo 0 centavos e
      extrato sem movimentos.
- [ ] Segundo pedido de abertura do mesmo titular devolve a carteira existente (mesmo
      identificador e mesmo saldo) e não cria segunda carteira.
- [ ] Depósito, saque, estorno, consulta de saldo e consulta de extrato em identificador não
      aberto são recusados com o motivo "carteira inexistente" (RN-12).
- [ ] Operação expressa em moeda diferente de BRL é recusada com o motivo "moeda divergente",
      sem conversão (RN-9).
- [ ] A moeda da carteira permanece BRL após qualquer sequência de operações.
- [ ] Tentativa de operar ou consultar carteira de outro titular é recusada (RN-14).
- [ ] O registro da abertura permanece no histórico e não pode ser alterado nem removido (RN-3).
- [ ] Nenhum campo de dado pessoal (nome, CPF, e-mail, telefone) é aceito ou armazenado na
      abertura.
- [ ] Após uma abertura recusada, nenhum identificador de carteira passa a existir.
- [ ] Uma carteira aberta aceita imediatamente um depósito válido.
