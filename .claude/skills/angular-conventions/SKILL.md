---
name: angular-conventions
description: Convenções de código, formulários, HTTP e testes para o frontend Angular do FinAgent. Use ao escrever componentes, formulários reativos, chamadas HTTP, tratamento de erro, ou testes de frontend, para manter consistência de estilo e qualidade.
---

# Convenções Angular (FinAgent)

Regras de estilo e qualidade. Para formulário reativo, interceptor ou teste, ABRA o
template correspondente e copie a estrutura.

## Invariantes (NUNCA quebre)

1. SEMPRE `changeDetection: OnPush` em todo componente.
2. SEMPRE control flow novo: `@if`, `@for`, `@switch`. NUNCA `*ngIf`/`*ngFor`.
3. NUNCA formulário template-driven. Sempre Reactive Forms TIPADO (`FormGroup<...>`).
4. NUNCA lógica de negócio no template — extraia para `computed`/método.
5. NUNCA trate erro HTTP componente a componente — um interceptor central mapeia
   erro HTTP → mensagem de domínio.
6. SEMPRE modele estado de UI explícito: `loading` / `error` / `data` via signals.
7. Dinheiro: exibe em BRL (pipe currency); envia ao backend em centavos (`number`).

## Nomenclatura

| Item        | Regra                          | Exemplo |
|-------------|--------------------------------|---------|
| Arquivo     | kebab-case + sufixo            | `wallet-details.page.ts`, `wallet.api.service.ts` |
| Classe      | PascalCase                     | `WalletDetailsPage` |
| Sufixos     | `.component`/`.service`/`.api.service`/`.page` | — |

## Templates (abra sob demanda)

| Vou escrever…            | Template |
|--------------------------|----------|
| Formulário reativo tipado + validação/erro | `reference/reactive-form.md` |
| Interceptor de erro/auth  | `reference/http-interceptor.md` |
| Teste de componente/service | `reference/testing.md` |

## Checklist antes de concluir

- [ ] `OnPush` e control flow novo (`@if`/`@for`)?
- [ ] Formulário reativo tipado, mensagens de erro via signal/computed?
- [ ] Erro HTTP tratado no interceptor, não no componente?
- [ ] Dinheiro exibido em BRL e enviado em centavos?
- [ ] Teste para toda página e todo service de API?
