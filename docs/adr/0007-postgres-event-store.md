# ADR-0007: PostgreSQL como Event Store (append-only)

- **Status:** Aceito
- **Data:** 2026-08-28

## Contexto
Precisamos de um armazenamento durável, transacional e com controle de
concorrência otimista para anexar eventos de agregados sem perder consistência.

## Decisão
Usar **PostgreSQL** com uma tabela append-only de eventos (por agregado, com número
de versão sequencial e checagem de versão esperada no append). Sem produto de event
store dedicado nesta fase — mantém o stack enxuto e demonstra o padrão na unha.

## Consequências
- (+) Transacional e amplamente conhecido; atende o requisito "banco relacional".
- (+) Concorrência otimista via constraint de (aggregate_id, version).
- (−) Sem features prontas de um EventStoreDB (subscriptions, projections nativas).

## Alternativas consideradas
- **EventStoreDB:** poderoso, mas adiciona um produto a operar e esconde o padrão.
- **SQL Server:** usaremos em OUTRO serviço, pra exibir os dois bancos relacionais.
