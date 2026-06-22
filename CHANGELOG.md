# Changelog

Todas as mudanças relevantes deste projeto serão registradas aqui. O formato segue Keep a Changelog e o versionamento segue Semantic Versioning.

## [Unreleased]

### Added

- Matriz automatizada para Keycloak 24, 25 e 26.
- Aplicação ASP.NET Core de referência com JWT, autorização por scopes, rate limiting, Problem Details e OpenTelemetry opcional.
- Módulos de Protocol Mappers, Identity Providers, Sessions e Events.
- Baseline automatizado da API pública.
- Pacote MCP opcional, read-only e isolado do SDK principal.
- Threat model de observabilidade e diagnóstico assistido por IA.

## [0.1.0-preview.1] - 2026-06-22

### Added

- SDK inicial para Realms, Users, Clients, Client Scopes, Roles e Groups.
- Contexto dinâmico e imutável de Realm.
- Client Credentials e Password Grant explicitamente habilitado.
- Resiliência via `IHttpClientFactory`, retry, circuit breaker e timeout.
- Logs estruturados, traces, métricas e diagnóstico sanitizado.
