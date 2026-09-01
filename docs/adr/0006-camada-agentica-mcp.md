# ADR-0006: Camada agêntica via MCP (Model Context Protocol)

- **Status:** Aceito
- **Data:** 2026-08-28

## Contexto
O diferencial do FinAgent é ser operável por linguagem natural. Precisamos expor
as capacidades do sistema para um LLM de forma padronizada, versionável e com
controle de permissões — não como prompt engineering ad-hoc chumbado no agente.

## Decisão
Publicar as operações do sistema como **tools de um MCP Server em C#** (padrão
aberto da Anthropic). Um **agente conversacional** atua como cliente MCP: recebe a
pergunta do usuário, deixa o LLM raciocinar e escolher tools, executa e responde.
Isso espelha, em pequena escala, a ponte MCP que a Sankhya usa no EIP Cognitivo.

## Consequências
- (+) Capacidades desacopladas do modelo: qualquer LLM/cliente compatível consome.
- (+) Catálogo de tools por domínio, com autorização aplicada na borda da tool.
- (+) Aderente ao requisito "interesse/conhecimento no ecossistema de IA".
- (−) Tecnologia em evolução rápida (SDK MCP em preview): fixar versões.

## Alternativas consideradas
- **Chamar o LLM direto com function-calling proprietário:** acopla ao fornecedor
  e não cria um catálogo de capacidades reutilizável. Descartado.
