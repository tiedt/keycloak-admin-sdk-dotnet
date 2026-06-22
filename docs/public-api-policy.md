# Política da API pública

## Estado atual

A superfície da release `0.1.0-preview.1` foi inventariada pelos analyzers `Microsoft.CodeAnalysis.PublicApiAnalyzers`. Interfaces setoriais, modelos de request/resource, exceções, configuração, extensões de DI e contratos de observabilidade compõem a API pública intencional.

Implementações HTTP, representações da API remota, handlers de autenticação, mappers e classes concretas de serviço permanecem internas.

## Regras de revisão

Toda alteração pública deve:

1. Ter justificativa de caso de uso e nome consistente com os módulos existentes.
2. Preservar cancelamento assíncrono e nullability.
3. Evitar expor modelos internos ou detalhes do JSON do Keycloak.
4. Incluir teste e documentação.
5. Atualizar o baseline público e o changelog deliberadamente.
6. Explicar impacto binário e de código-fonte.

O CI trata adição ou remoção não declarada como erro. A API fica em preview até haver uso real suficiente para congelamento `1.0.0`.
