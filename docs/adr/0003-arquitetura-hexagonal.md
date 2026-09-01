# ADR-0003: Arquitetura Hexagonal (Ports & Adapters)

- **Status:** Aceito
- **Data:** 2026-08-28

## Contexto
Queremos que a regra de negócio não dependa de Postgres, Kafka, Mongo ou de
qualquer framework. Isso mantém o domínio testável e permite trocar tecnologia
sem reescrever o núcleo.

## Decisão
Organizar cada serviço em camadas com dependências apontando pra dentro:
**Domain** (núcleo, sem dependências) → **Application** (casos de uso, define as
*portas*) → **Infrastructure/Api** (*adapters* que implementam as portas).
Ex.: `IEventStore` é porta no Domain; `PostgresEventStore` é adapter na Infra.

## Consequências
- (+) Domínio 100% testável sem banco nem broker.
- (+) Trocar Mongo por Redis, ou REST por gRPC, não toca a regra de negócio.
- (−) Mais projetos e indireções que uma solução em camada única.

## Alternativas consideradas
- **N-tier clássico (UI→BLL→DAL):** dependências apontam pra fora; a regra acaba
  acoplada ao ORM/banco. Descartado.
