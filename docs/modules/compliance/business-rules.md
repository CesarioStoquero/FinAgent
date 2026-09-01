# Regras de negócio — Módulo Compliance / LGPD

Fonte da verdade de compliance de dados pessoais. Regras que TODA feature que toque dado
pessoal (nome, CPF, e-mail, telefone, etc.) deve respeitar. Referenciadas por ID (LGPD-x).

> ⚠ Todas as LGPD-x estão **⚠ a confirmar** no catálogo `docs/business-rules.md` (falta ADR
> + validação jurídica). São recomendação forte, não lei que trava o projeto — ainda.

> ⚠️ Isto é orientação de ENGENHARIA, não parecer jurídico. Os números de artigo da LGPD
> abaixo são referência; a área jurídica do FinAgent valida a interpretação final.

> **O conflito central deste projeto:** o ledger é Event Sourcing — eventos IMUTÁVEIS para
> sempre (ver [wallet] RN-3). A LGPD dá ao titular o **direito de eliminação** (art. 18, VI).
> Imutável × "apague meus dados" = tensão real. As regras abaixo resolvem isso sem quebrar
> o ledger.

## Regras (invariantes de compliance)

- **LGPD-1 (PII fora do evento — minimização):** eventos de domínio NUNCA carregam dado
  pessoal. Só IDs/referências (`WalletId`, `CustomerId`). O dado pessoal vive num store
  separado, mutável e apagável. (Padrão *Forgettable Payloads*; art. 6 — necessidade.)
- **LGPD-2 (Direito de eliminação):** o sistema DEVE conseguir tornar o PII de um titular
  inacessível a pedido, SEM apagar/alterar eventos do ledger. (Art. 18, VI.)
- **LGPD-3 (Crypto-shredding):** quando PII precisar estar no fluxo de eventos, ele é
  cifrado com uma chave POR TITULAR. "Eliminar" = destruir a chave → o dado fica
  permanentemente ilegível, e o evento continua íntegro. (Padrão *Crypto-Shredding*.)
- **LGPD-4 (Retenção legal sobrepõe eliminação):** dado TRANSACIONAL/financeiro tem
  obrigação legal de guarda e permanece (art. 16, I). Separe claramente: o *valor/lançamento*
  é retido; o que se elimina é o *PII vinculável* ao titular. Nunca apague o lançamento.
- **LGPD-5 (Anonimização):** dado anonimizado de forma irreversível deixa de ser dado
  pessoal e pode ser retido em relatórios/agregados (art. 5, III; art. 12).
- **LGPD-6 (Eliminação propaga para leitura):** ao eliminar, o PII some também dos read
  models (Mongo) e projeções. "Esquecer" pode ser modelado como um evento que as projeções
  tratam removendo/mascarando o PII no read model.
- **LGPD-7 (Base legal e consentimento):** todo tratamento de PII tem base legal (art. 7).
  Se a base for consentimento, ele é revogável — e a revogação dispara o fluxo de
  eliminação (LGPD-2).
- **LGPD-8 (Segurança em repouso):** PII é cifrado em repouso e no transporte (art. 46).
- **LGPD-9 (Sem PII "por via das dúvidas"):** logs, mensagens de erro e payloads de Kafka
  não viram depósito acidental de PII. Só se coleta o necessário para a finalidade.

## Como usar

- **Spec (`nova-spec`):** se a feature coleta/exibe dado pessoal, marque quais LGPD-x se
  aplicam e trate consentimento/eliminação como requisito, não detalhe.
- **Plano (`software-engineer`):** desenhe onde o PII mora (fora do evento — LGPD-1) e como
  a eliminação acontece (LGPD-2/3/6) ANTES de propor o modelo de eventos.
- **Implementação/Revisão:** o `compliance-specialist` confere o código contra LGPD-1..9.
  Achou CPF/nome dentro de um evento? Violação de LGPD-1, crítico.

## Fontes

- Lei 13.709/2018 (LGPD) — arts. 6, 7, 12, 16, 18, 46.
- Mathias Verraes — *Eventsourcing Patterns: Forgettable Payloads* e *Crypto-Shredding
  (Throw Away the Key)*.
- Oskar Dudycz (event-driven.io) — *How to deal with privacy and GDPR in Event-Driven systems*.
- Wikipedia — *Crypto-shredding*.
