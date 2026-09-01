---
name: dotnet-testing
description: Padrões de teste automatizado do FinAgent em .NET (xUnit v3, AwesomeAssertions, Testcontainers, TDD). Use ao escrever testes de unidade ou integração, ao implementar uma feature seguindo TDD, ou ao decidir o que e como testar em domínio, handlers, event store e projeções.
---

# Testes automatizados (FinAgent)

**Stack fixada (ADR-0009):** .NET 10 (`net10.0`), C# 14, **xUnit v3**, **AwesomeAssertions**,
**Testcontainers**. NUNCA use FluentAssertions (licença comercial desde a v8); AwesomeAssertions
é fork com API idêntica — a sintaxe `.Should().Be()` dos templates continua valendo.

Ferramentas: **xUnit v3** + **AwesomeAssertions** (unidade) e **Testcontainers** (integração
com Postgres/Mongo/Kafka reais). TDD é padrão.

Antes de escrever o teste, decida o TIPO (tabela) e ABRA o template correspondente.

## Ciclo TDD (obrigatório)

1. Vermelho — escreva o teste que descreve o comportamento. Ele DEVE falhar primeiro.
2. Verde — implemente o mínimo para passar.
3. Refatore — sem mudar comportamento. NUNCA escreva produção sem teste que a justifique.

## Que teste é este? (tabela de decisão)

| Testando…                          | Tipo        | Sem/Com banco | Template |
|------------------------------------|-------------|---------------|----------|
| Regra do agregado (saque, saldo)   | Unidade     | SEM banco     | `reference/unit-domain.md` |
| Command/Query handler              | Unidade     | portas mockadas| `reference/unit-domain.md` |
| Event store (append/load/concorr.) | Integração  | Testcontainers PG | `reference/integration.md` |
| Projeção populando read model      | Integração  | Testcontainers Mongo+Kafka | `reference/integration.md` |

## Invariantes (NUNCA quebre)

1. NUNCA mocke banco/broker em teste de INTEGRAÇÃO. Use Testcontainers (container real).
2. NUNCA use `Thread.Sleep` fixo esperando consistência. Faça poll com timeout de condição.
3. UM assert lógico por teste (pode ser multi-linha via AwesomeAssertions).
4. Nome do teste descreve a regra: `Metodo_Cenario_ResultadoEsperado`
   (ex.: `Withdraw_ComSaldoInsuficiente_LancaDomainException`).
5. Todo command/query handler tem AO MENOS um teste de caminho feliz + um de falha/borda.
6. Cada teste de integração é isolado: schema/coleção limpos por teste, ou container por classe.

## O que testar no domínio (event-sourced)

Padrão arrange-act-assert em cima de EVENTOS, não de estado persistido:
- Arrange: reidrate o agregado a partir de um histórico de eventos.
- Act: chame o comando (`Deposit`/`Withdraw`).
- Assert: verifique `UncommittedEvents` (o evento certo foi levantado?) E o estado (saldo).

Casos de borda que SEMPRE precisam de teste: saque > saldo, valor <= 0, moeda divergente.

## Checklist antes de concluir

- [ ] O teste falhou antes de existir a implementação (vermelho de verdade)?
- [ ] Handler tem caminho feliz + falha?
- [ ] Integração usa container real, sem `Sleep` fixo?
- [ ] Nome do teste comunica a regra?
