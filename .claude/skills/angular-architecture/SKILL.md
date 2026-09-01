---
name: angular-architecture
description: Padrões de arquitetura para o frontend Angular do FinAgent. Use ao criar aplicação, componentes, features, rotas ou serviços Angular, ao estruturar pastas, ou ao decidir gerência de estado e comunicação com a API do backend.
---

# Arquitetura Angular (FinAgent)

Alvo: Angular moderno (standalone + signals). Sem NgModules legados.

Antes de criar um arquivo, decida a PASTA (tabela) e o TIPO. Para service de API ou
página com estado, ABRA `reference/feature-structure.md` e copie a estrutura.

## Invariantes (NUNCA quebre)

1. NUNCA use `NgModule`. Sempre standalone components.
2. NUNCA use `RxJS` para estado. Estado = signals; `computed`/`effect` para derivado.
   RxJS SÓ na borda de I/O (HTTP), convertido para signal com `toSignal`.
3. NUNCA acesse `HttpClient` fora de um `*.api.service.ts`. Componente não chama API direto.
4. NUNCA ponha lógica de estado num componente de `ui/` (dumb). Ele só recebe `input()`/emite `output()`.
5. NUNCA use `any`. DTOs explícitos e tipados.
6. SEMPRE carregue rotas de feature por lazy (`loadChildren`/`loadComponent`).

## Onde mora cada coisa? (tabela de decisão)

| Estou criando…                    | Pasta                    | Tipo |
|-----------------------------------|--------------------------|------|
| Service singleton, interceptor, guard | `core/`              | infra de app |
| Componente/pipe/diretiva reutilizável, sem estado | `shared/`| burro |
| Service de acesso à API + DTOs    | `features/<x>/data/`     | `*.api.service.ts` |
| Componente de apresentação (dumb) | `features/<x>/ui/`       | input/output |
| Componente de rota (smart)        | `features/<x>/pages/`    | orquestra estado |
| Rotas da feature                  | `features/<x>/<x>.routes.ts` | lazy |

## Estrutura por feature (canônica)

```
src/app/
  core/          # singletons, interceptors, guards, config
  shared/        # reutilizáveis sem estado
  features/
    wallet/
      data/      # wallet.api.service.ts + models (DTOs)
      ui/        # componentes dumb (input/output)
      pages/     # componentes smart (estado, chamam services)
      wallet.routes.ts
  app.routes.ts
  app.config.ts
```

## Smart vs Dumb (a divisão que a IA erra)

- `pages/` (smart): injeta o api.service, mantém signals de estado (`loading`/`error`/`data`),
  passa dados para os dumb via `input()`, reage a `output()`.
- `ui/` (dumb): zero injeção de service de API. Recebe tudo por `input()`, avisa por `output()`.
  Se um componente de `ui/` chama `HttpClient`, está errado — suba a chamada para a página.

## Comunicação com o backend

- Um `*.api.service.ts` por agregado (`wallet.api.service.ts`).
- `HttpClient` tipado; erro tratado por interceptor central (ver skill `angular-conventions`).
- Agente conversacional consumido por `agent.service.ts` dedicado (streaming quando houver).

## Checklist antes de concluir

- [ ] Componente é standalone e a rota é lazy?
- [ ] Estado em signals; RxJS só na borda de HTTP?
- [ ] Acesso à API isolado no `*.api.service.ts`?
- [ ] Dumb em `ui/` sem service; smart em `pages/`?
