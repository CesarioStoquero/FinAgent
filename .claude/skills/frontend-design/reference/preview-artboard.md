# Gerar as 2 direções em 2 links (o passo de escolha)

Objetivo: o usuário VÊ 2 direções aplicadas ao FinAgent, em 2 links separados, e escolhe.
Fidelidade de DECISÃO, não de entrega: o preview mostra a estética numa tela-vitrine (o
dashboard da carteira costuma ser a melhor), não o app inteiro.

## Procedimento

1. Pegue as 2 direções escolhidas pela heurística (SKILL.md) no `catalog.md`.
   (Catálogo vazio → PARE e peça curadoria; não invente.)
2. Para CADA direção, monte a tela-vitrine do FinAgent aplicando os tokens da direção:
   conteúdo REAL do domínio (saldo em BRL + centavos, extrato denso com id de evento,
   pipeline CQRS, contexto BR/Pix/LGPD). Nunca lorem ipsum.
3. Publique como 2 previews em 2 LINKS separados (um por direção), para comparar lado a lado.
   Rotule "Direção A — <id>" e "Direção B — <id>", com a motivação e o trade-off honesto de
   cada uma (não faça o voto rigado: as duas ganham uma defesa real).
   - O mecanismo de preview (canvas/HTML publicado) é do agente PRINCIPAL. Se um subagente
     montar os arquivos, ele ENTREGA os arquivos e o principal publica os 2 links.
4. Nomes das direções são ESTÁVEIS: "Direção A" continua sendo A entre turnos.

## Depois da escolha
Grave: ADR-0010 (direção vencedora; a perdedora vira "alternativa considerada") e materialize
`src/styles/tokens.css` (ver `design-tokens.md`). A partir daí, `frontend-engineer` constrói
contra o TOKEN — não contra o preview.
