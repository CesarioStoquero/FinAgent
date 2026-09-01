---
name: dotnet-event-sourcing-cqrs
description: Padrões de Event Sourcing e CQRS para o FinAgent em .NET. Use ao criar agregados, eventos de domínio, command/query handlers, event store, projeções ou read models. Aplica quando a tarefa envolve carteira, ledger, saldo, extrato, depósito, saque ou qualquer mudança de estado que deva virar evento.
---

# Event Sourcing + CQRS (FinAgent)

Referência: ADR-0001, ADR-0002, ADR-0007, ADR-0005.

Antes de escrever código, identifique O QUE você está criando e siga a linha da tabela.
Se for um agregado, evento ou handler, ABRA o template canônico correspondente e copie
a estrutura — não invente assinatura nova.

## Invariantes (NUNCA quebre)

1. NUNCA persista o estado do agregado direto. O estado é derivado do replay dos eventos.
2. NUNCA use `double`/`float`/`decimal` em valor monetário. SEMPRE `Money` em centavos (`long`).
3. NUNCA ponha regra de negócio no handler. Regra mora no agregado; handler só orquestra.
4. NUNCA reidrate eventos para responder query. Query lê SEMPRE do read model (Mongo).
5. NUNCA nomeie evento no presente/imperativo. Evento é fato no passado: `TransactionPosted`.
6. NUNCA dê comportamento ao evento. Evento é `record` imutável só com dados.
7. SEMPRE faça `AppendAsync` com `expectedVersion` (concorrência otimista, ADR-0007).
8. SEMPRE publique os eventos no Kafka DEPOIS do append e então `ClearUncommittedEvents`.
9. PARTIDA DOBRADA (ADR-0008): todo movimento de dinheiro é UM `TransactionPosted` com
   lançamentos que SOMAM ZERO (débitos == créditos). NUNCA poste um lado só. A conta do
   usuário e a de contrapartida do sistema são os dois lados.

## O que estou criando? (tabela de decisão)

| Criando…              | Camada          | Regra-chave                                              | Template |
|-----------------------|-----------------|----------------------------------------------------------|----------|
| Agregado `Account`    | Domain          | Muda estado só via `Raise(evento)` + `Apply`; valida antes | `reference/aggregate.md` |
| Evento / `TransactionPosted` | Domain   | `record` no passado; lançamentos que somam zero (RN-1)   | `reference/aggregate.md` |
| Value object (Money)  | Domain          | Centavos `long`; operar moedas diferentes lança exceção  | `reference/aggregate.md` |
| Command + handler     | Application     | load → reidrata → regra → append → publish → clear       | `reference/command-handler.md` |
| Query + handler       | Application     | Lê do read model; NUNCA toca event store                 | `reference/query-handler.md` |
| Projeção / read model | Infrastructure  | Consome evento do Kafka → upsert no Mongo (idempotente)  | `reference/projection.md` |

## Fluxo obrigatório de um command handler

```
1. Carregar histórico:  var eventos = await store.LoadAsync(id);
2. Reidratar:           var agg = Account.Rehydrate(eventos);  // aggregate replaya no ctor
3. Executar regra:      agg.Withdraw(money);                    // agg valida, monta soma-zero e Raise(...)
4. Append otimista:     await store.AppendAsync(id, agg.UncommittedEvents, agg.Version);
5. Publicar:            await publisher.PublishAsync(agg.UncommittedEvents);
6. Limpar:              agg.ClearUncommittedEvents();
```
Faltou um passo → está errado. A ordem 4→5→6 é fixa.

## Consistência

Read model é eventualmente consistente (projeção assíncrona via Kafka). Isso é aceitável
para saldo/extrato. Se um caso de uso exigir leitura forte, PARE e pergunte — não reidrate
o agregado numa query para "resolver".

## Checklist antes de concluir

- [ ] Nenhum estado mutável é fonte da verdade (saldo é derivado dos eventos)?
- [ ] Todo valor monetário é `Money` (centavos)?
- [ ] Handler de command segue a ordem 1→6 acima?
- [ ] Query lê do read model, sem tocar o event store?
- [ ] Teste de caminho feliz + teste de falha para cada handler? (ver skill `dotnet-testing`)
