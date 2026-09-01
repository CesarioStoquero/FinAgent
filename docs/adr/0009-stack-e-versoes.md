# ADR-0009: Stack e versões de plataforma

- **Status:** Aceito
- **Data:** 2026-09-01

## Contexto
O projeto é greenfield: `src/` e `tests/` têm as pastas dos seis projetos, mas nenhum
`.csproj` — nada foi compilado ainda. O `Directory.Build.props` estava fixado em
**`net8.0`**, cujo suporte LTS **termina em 10/11/2026**: fundar o projeto ali significaria
nascer fora de suporte antes do primeiro release.

As skills do harness (`.claude/skills/`) também assumiam implicitamente versões anteriores
(Karma/Jasmine no Angular, `toSignal` como padrão de leitura HTTP, FluentAssertions). Como
os agentes copiam os templates canônicos de `reference/` à risca, qualquer versão errada
aqui se propaga por obediência para toda feature futura. Daí a decisão vir ANTES do
esqueleto de código.

## Decisão

### Backend — .NET 10 (LTS) / C# 14
- `TargetFramework` = **`net10.0`**, suportado até **10/11/2028**.
- O .NET 11 (nov/2026) é **STS** (18 meses) — descartado: backend financeiro fica em LTS.
- OpenAPI pelo **`AddOpenApi()`/`MapOpenApi()` nativos**. **Proibido Swashbuckle** em
  projeto novo (é a orientação oficial a partir do .NET 9).
- Testes: **xUnit v3** + **AwesomeAssertions** + **Testcontainers**.
  - AwesomeAssertions é fork open-source do FluentAssertions v7 com **API idêntica**
    (`.Should().Be()`), adotado porque o FluentAssertions v8 mudou para licença comercial.
    Os templates de teste em `reference/` seguem válidos sem reescrita.

### Frontend — Angular 22
- **Angular 22** (estável em 03/06/2026), **LTS até maio/2028**. Com a cadência anual do
  Angular, a v23 só chega em junho/2027 — a v22 é estável por todo o horizonte do projeto.
- Runner de teste: **Vitest**. O Karma está descontinuado e não é opção.
- HTTP: **`httpResource` para leitura** (saldo, extrato — já entrega `loading`/`error`/
  `value` como signals) e **`HttpClient` para escrita** (saque, depósito). Substitui o
  padrão `Observable` + `toSignal`, que é da era v16.
- Formulários: **Signal Forms** (estável na v22). ⚠ *Assumido, pendente de confirmação
  explícita.* Reverter para Reactive Forms tipado é contido: afeta
  `angular-conventions/SKILL.md` e `reference/reactive-form.md`.
- Nomenclatura: **mantidos os sufixos tradicionais** (`.component.ts`, `.service.ts`,
  `.page.ts`). É a recomendação padrão do guia oficial do Angular quando não há razão
  específica para o estilo "intent over role" da v20+.

## Consequências
- (+) Toda a plataforma em LTS até 2028; nenhuma migração forçada no horizonte do projeto.
- (+) As skills passam a poder se apoiar nas skills oficiais (`dotnet/skills`,
  `angular/skills`), que assumem exatamente essas versões.
- (+) Elimina a dívida de licença do FluentAssertions antes de existir a primeira linha.
- (−) Signal Forms e `httpResource` são APIs recentes (estáveis desde jun/2026): menos
  material de terceiros e menos respostas prontas em fórum. Mitigado pelo template
  canônico em `reference/` e pela skill oficial do Angular.
- (−) Exige revisar as skills antes do esqueleto: `dotnet-testing` (xUnit v3,
  AwesomeAssertions), `angular-architecture` (`httpResource`), `angular-conventions`
  (Vitest, Signal Forms). Enquanto isso não for feito, os templates estão desalinhados
  desta ADR.

## Alternativas consideradas
- **Manter `net8.0`:** descartado — sai de suporte em 10/11/2026.
- **.NET 11 (nov/2026):** descartado — STS de 18 meses, e ainda não lançado.
- **Angular 21:** descartado — LTS termina antes (maio/2027) e não tem Signal Forms,
  Angular Aria nem signals assíncronos estáveis.
- **FluentAssertions fixado na v7:** descartado — congela a biblioteca e só adia a decisão.
- **Shouldly:** viável e MIT, mas exigiria reescrever os templates de teste; o ganho sobre
  AwesomeAssertions não paga o custo.
