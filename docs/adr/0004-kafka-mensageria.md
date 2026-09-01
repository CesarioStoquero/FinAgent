# ADR-0004: Kafka como backbone de eventos entre serviços

- **Status:** Aceito
- **Data:** 2026-08-28

## Contexto
Os microsserviços precisam reagir a fatos de outros serviços (ex.: notificação
reage a um saque) sem acoplamento síncrono. Também queremos um log de eventos
durável e reprocessável, coerente com Event Sourcing.

## Decisão
Usar **Apache Kafka** como broker. Eventos de domínio publicados pelo serviço de
escrita viram mensagens em tópicos; consumidores (projeções, notificações,
serviços downstream) processam de forma assíncrona.

## Consequências
- (+) Log durável e particionado, com replay — combina com Event Sourcing.
- (+) Alta vazão e desacoplamento temporal entre produtores/consumidores.
- (−) Operacionalmente mais pesado que RabbitMQ (mais peças pra rodar).
- (−) Exige cuidado com ordenação por partição e idempotência no consumidor.

## Alternativas consideradas
- **RabbitMQ:** mais simples de operar, mas sem o log reprocessável nativo que
  o Kafka oferece. Kafka também é mais aderente ao domínio financeiro/streaming.
