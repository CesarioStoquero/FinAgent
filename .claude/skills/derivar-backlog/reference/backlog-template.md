# Template canônico — docs/backlog.md

Copie esta estrutura. Não invente seções nem renomeie campos: o `BL-x` e os nomes de campo
são citados de fora do arquivo (specs, tasks, conversa).

---

# Backlog do FinAgent

> Derivado por `/derivar-backlog` em AAAA-MM-DD.
> Fonte: `docs/business-rules.md` + `docs/modules/*/business-rules.md` + `docs/adr/`.
> Item aqui é **candidato**, não compromisso. Vira trabalho quando você roda a entrada dele.

## Legenda

- **Tipo:** `capacidade` (comportamento — vira spec via `/nova-spec`) · `chore` (scaffold,
  infra, CI — vai direto ao `software-engineer`, sem spec)
- **Estado:** `pendente` · `spec escrita` · `em entrega` · `entregue`
- **Regra ✓** é lei; **regra ⚠** ainda depende da sua decisão (o item nasce com pendência).

## Itens

### BL-1 — <nome da capacidade, em linguagem de usuário>

- **Tipo:** capacidade · **Módulo:** wallet · **Estado:** pendente
- **O quê:** <1-2 frases de comportamento observável. Sem termo técnico.>
- **Por quê:** <o que destrava ou que valor entrega>
- **Satisfaz:** RN-12 ✓, RN-1 ✓
- **Restringido por:** ADR-0001, ADR-0008
- **Depende de:** BL-0 (ou "nada")
- **Decisões abertas:** `[NEEDS CLARIFICATION: …]` (ou "nenhuma")
- **Entrada:** `/nova-spec "<frase que descreve o item>"`

### BL-2 — <chore>

- **Tipo:** chore · **Módulo:** — · **Estado:** pendente
- **O quê:** <o que precisa existir para o resto andar>
- **Por quê:** <o que está bloqueado sem isso>
- **Satisfaz:** — (chore não satisfaz regra de negócio)
- **Restringido por:** ADR-0009 (stack), ADR-0003 (camadas)
- **Depende de:** nada
- **Decisões abertas:** nenhuma
- **Entrada:** `software-engineer` direto (chore não passa por spec)

## Ordem sugerida

1. **BL-2** — chore que bloqueia todo o resto.
2. **BL-1** — destrava BL-3 e BL-4.
3. …

> Critério do ranking: dependência primeiro (o que destrava mais), valor depois.

## Bloqueios ativos

| Bloqueio | Afeta | Como destravar |
|----------|-------|----------------|
| ADR-0010 com Status "Proposto" | todo item de frontend | rodar `/definir-design` |

## Lacunas do catálogo (regra faltando)

O que o projeto claramente PRETENDE fazer mas nenhuma regra de negócio sustenta. Não vira
item (invariante 6 — não se inventa regra); vira decisão sua: escrever a regra em
`docs/business-rules.md` ou aceitar que a capacidade não existe.

| Lacuna | Evidência de que se pretende | O que falta |
|--------|------------------------------|-------------|
| <capacidade sem RN> | <ADR/README que a promete> | uma regra em `docs/business-rules.md` |

## Fora do backlog (e por quê)

Rastreabilidade deliberada: o que existe no conhecimento do projeto e **não** virou item.

| ADR / Regra | Por que não virou item |
|-------------|------------------------|
| ADR-0003 (hexagonal) | restrição estrutural sem comportamento observável — aplica a TODO item |
| ADR-0009 (stack) | restrição de versão — vale para todo item, nunca é trabalho isolado |
| RN-7 ⚠ | regra ainda não confirmada — só vira item se você bater o martelo |
