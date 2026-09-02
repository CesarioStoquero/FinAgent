# Spec: <nome da feature>

- **ID:** SPEC-<AAAA-MM-DD>-<slug>  ·  **Status:** Rascunho | Validada | Em entrega | Entregue
- **Autor:** <você>  ·  **Data:** AAAA-MM-DD

> Esta spec descreve O QUE o sistema deve fazer e POR QUÊ — nunca o COMO técnico
> (agregado, tabela, endpoint). O "como" é decidido pelo software-engineer no plan.md.
> Tudo que você não souber responder vira `[NEEDS CLARIFICATION: pergunta]`.

## 1. Objetivo (por quê)
Qual problema do usuário isto resolve e por que importa. 2-3 frases.

## 2. Atores
Quem dispara e quem é afetado (ex.: titular da carteira, sistema de conciliação,
o agente de linguagem natural).

## 3. Escopo
- **Entra:** o que esta feature entrega.
- **NÃO entra:** o que fica de fora de propósito (evita o agente inventar escopo).

## 4. Cenários (comportamento observável)
Descreva em Dado / Quando / Então. Um caminho feliz + os casos de borda.

- **Cenário feliz**
  - Dado <estado inicial>
  - Quando <ação do ator>
  - Então <resultado observável>
- **Borda / erro 1**
  - Dado … Quando … Então <erro/recusa esperada>
- (repita para cada regra de borda: valor <= 0, saldo insuficiente, moeda divergente…)

## 5. Requisitos funcionais
Lista numerada, testável, imperativa. Cada item = uma frase verificável.

- **RF-1:** O sistema DEVE …
- **RF-2:** O sistema DEVE recusar … quando …
- **RF-3:** …

## 6. Regras de negócio (invariantes do domínio)
Regras que valem sempre, independentes de tela. Para o FinAgent, seja explícito sobre:
- **Dinheiro:** valores em centavos (inteiro). Nunca fração de centavo.
- **Moeda:** operações entre moedas diferentes são proibidas? (sim, por padrão)
- **Saldo:** pode ficar negativo? (não, por padrão) Qual o limite?
- Outras invariantes específicas desta feature.

## 7. Restrições herdadas (ADR-x / RN-x)
As decisões e regras JÁ tomadas que esta feature deve respeitar. Cite por ID — não repita o
conteúdo; o agente navega até a fonte. Regra **✓** é lei e vira RF-x/critério de aceite;
regra **⚠** NUNCA entra aqui como lei — vira pendência na seção 10.

| ID | O que impõe A ESTA feature | Fonte |
|----|----------------------------|-------|
| RN-x ✓ | <a exigência concreta, em uma linha> | `docs/business-rules.md` |
| LGPD-x ⚠ | <só se a feature tocar dado pessoal> | `docs/modules/compliance/business-rules.md` |
| ADR-000x | <a restrição que a decisão impõe> | `docs/adr/000x-….md` |

## 8. Camada agêntica (linguagem natural)
Preencher só se a feature deve ser operável pelo agente (ADR-0006).
- Quais operações viram **tools MCP**? (ex.: "sacar", "consultar saldo")
- Como o usuário pediria isso em linguagem natural? (ex.: "tira 50 reais da minha carteira")
- Restrições/autorização na borda da tool.
- Se não se aplica: escreva "Não se aplica".

## 9. Suposições e fora de escopo
- Suposições que você está assumindo como verdade.
- O que explicitamente NÃO será feito agora.

## 10. Pendências de decisão
Liste aqui tudo que precisa da SUA decisão antes de construir. O software-engineer
não avança enquanto houver item aberto.
- `[NEEDS CLARIFICATION: …]`

## 11. Critérios de aceite
Como saber que está pronto. Devem espelhar os cenários da seção 4, verificáveis por teste.
- [ ] <condição observável 1>
- [ ] <condição observável 2>
