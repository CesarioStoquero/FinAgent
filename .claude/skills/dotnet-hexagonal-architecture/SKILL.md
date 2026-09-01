---
name: dotnet-hexagonal-architecture
description: Padrões de Arquitetura Hexagonal (Ports & Adapters) para os serviços .NET do FinAgent. Use ao criar ou modificar projetos, definir camadas, mover dependências, criar interfaces (portas) ou adapters, ou revisar se a regra de negócio está desacoplada de infraestrutura.
---

# Arquitetura Hexagonal em .NET (FinAgent)

Referência: ADR-0003. Regra inegociável: **dependências apontam para dentro**.

Antes de criar uma classe, decida em QUAL camada ela mora (tabela abaixo). Se for um
projeto novo ou wiring de DI, ABRA `reference/project-layout.md` e copie a estrutura.

## Invariantes (NUNCA quebre)

1. `Domain` NUNCA referencia outro projeto. Zero dependência de framework/infra.
2. `Application` referencia SÓ `Domain`.
3. `Infrastructure` e `Api` referenciam `Application` e `Domain` (nunca o contrário).
4. Regra de negócio NUNCA importa `Npgsql`, `MongoDB.Driver`, `Confluent.Kafka`,
   `Microsoft.EntityFrameworkCore` etc. Precisou de infra? Defina uma PORTA no Domain.
5. Porta (interface) mora no `Domain`. Adapter (implementação) mora na `Infrastructure`.
6. `internal` por padrão. `public` só no que cruza a fronteira do projeto.
7. Injeção por construtor (primary constructor). NUNCA service locator / `new` de adapter.

## Onde mora cada coisa? (tabela de decisão)

| Estou criando…                          | Vai em          | Por quê |
|-----------------------------------------|-----------------|---------|
| Agregado, evento, value object          | `*.Domain`      | Núcleo, sem dependência |
| Porta (`IEventStore`, `IWalletReadModel`)| `*.Domain`      | O núcleo declara o que precisa |
| Command/Query + handler                 | `*.Application` | Caso de uso; orquestra o domínio via portas |
| Adapter Postgres/Mongo/Kafka            | `*.Infrastructure` | Implementa a porta com tecnologia real |
| Endpoint HTTP (Minimal API)             | `*.Api`         | Traduz HTTP → command/query |
| `DependencyInjection.cs` de cada camada | a própria camada| Cada camada registra seus próprios serviços |

## Teste rápido: "isso viola a regra de dependência?"

Pergunte: "esta classe do Domain/Application menciona um tipo de banco/broker/HTTP?"
- Sim → ERRADO. Extraia uma porta no Domain e mova a implementação para Infrastructure.
- Não → ok.

## Checklist antes de concluir

- [ ] A regra nova está no Domain, não na Application/Infra?
- [ ] Toda dependência de infra passa por uma porta declarada no Domain?
- [ ] `Domain.csproj` não tem `<ProjectReference>` nem pacote de infra?
- [ ] `dotnet build` limpo com warnings-as-errors?
