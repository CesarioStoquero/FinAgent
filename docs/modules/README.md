# Módulos do FinAgent

O harness tem dois eixos: PAPEL (agentes que planejam/implementam/revisam) e MÓDULO
(o domínio). Este diretório é o eixo de módulo — cada módulo é uma fronteira de negócio,
com suas regras num arquivo próprio e um agente especialista que as guarda.

## Módulos atuais

| Módulo | Regras de negócio | Especialista |
|--------|-------------------|--------------|
| wallet (carteira/ledger) | `docs/modules/wallet/business-rules.md` | agente `wallet-specialist` |
| compliance (LGPD) — TRANSVERSAL | `docs/modules/compliance/business-rules.md` | agente `compliance-specialist` |

O módulo **compliance** é transversal: não é um domínio de negócio, mas um conjunto de
regras (LGPD) que se aplica a QUALQUER feature que toque dado pessoal. Rode o
`compliance-specialist` sempre que houver PII, além do especialista do domínio.

## Como adicionar um módulo novo (clone o padrão)

1. Crie `docs/modules/<modulo>/business-rules.md` com as regras de negócio do domínio,
   numeradas por ID (`RN-1`, `RN-2`, …) e com as fontes citadas. Regra = o que é sempre
   verdade no mundo real, não implementação.
2. Crie `.claude/agents/<modulo>-specialist.md` copiando `wallet-specialist.md`: troque o
   nome, a descrição e o caminho do business-rules que ele lê. Ele continua somente-leitura
   e responde/valida por ID de regra.
3. Adicione a linha do módulo na tabela acima.
4. Pronto: o `software-engineer` passa a consultar as regras do módulo no planejamento e o
   `deliver-feature` usa o especialista como porta de validação de domínio.

## Princípio

Regra de NEGÓCIO (RN-x, aqui) responde "o dinheiro/domínio está correto?".
Regra de ARQUITETURA (skills em `.claude/skills/`) responde "o código está bem-feito?".
São coisas diferentes e ficam em lugares diferentes de propósito.
