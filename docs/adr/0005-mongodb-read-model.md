# ADR-0005: MongoDB como store do read model (extrato)

- **Status:** Aceito
- **Data:** 2026-08-28

## Contexto
O read model do extrato é uma projeção denormalizada, orientada a leitura por
período e paginação. Não precisa de JOINs nem de transações multi-tabela; precisa
de documentos flexíveis e leitura rápida.

## Decisão
Persistir o read model em **MongoDB**, alimentado pelas projeções que consomem os
eventos do Kafka. A escrita (event store) permanece em PostgreSQL (ADR-0007).

## Consequências
- (+) Demonstra modelagem relacional (escrita) E não-relacional (leitura) no mesmo
      sistema — requisito explícito da vaga.
- (+) Documento de extrato mapeia bem pra resposta da API/agente.
- (−) Consistência eventual e ausência de garantias transacionais entre os dois
      stores (aceitável nesse read model).

## Alternativas consideradas
- **Redis:** ótimo pra cache/saldo, fraco pra consulta de extrato por período.
- **Postgres também na leitura:** válido, mas não exercita o "não-relacional".
