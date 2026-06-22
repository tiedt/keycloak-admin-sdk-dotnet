# Keycloak.AdminSdk — documentação

`Keycloak.AdminSdk` é uma biblioteca .NET 10 para administrar Keycloak por meio da Admin REST API com contratos fortemente tipados, DI nativa, troca segura de Realm, resiliência e telemetria opcional.

## O que o pacote resolve

- Autenticação administrativa com Client Credentials ou Password Grant legado.
- Cache concorrente e renovação antecipada de access tokens.
- Administração de Realms, Users, Clients, Client Scopes, Roles, Groups, Protocol Mappers, Identity Providers, Sessions e Events.
- Realm Roles, Client Roles e mappings para Users e Groups.
- Associação de Users a Groups e Client Scopes a Clients.
- Contextos imutáveis para trabalhar com múltiplos Realms simultaneamente.
- Provisionamento fluído de Clients com compensação em caso de falha.
- Retry exponencial, jitter, circuit breaker e timeouts.
- Logs estruturados, traces, métricas e eventos diagnósticos compatíveis com OpenTelemetry.
- Ponto de extensão seguro para análise de incidentes por IA.

## Navegação

1. [Instalação e primeiro uso](getting-started.md)
2. [Configuração completa](configuration.md)
3. [Guia dos módulos](modules.md)
4. [OpenTelemetry e logs](observability.md)
5. [Segurança](security.md)
6. [Compatibilidade](compatibility.md)
7. [Cobertura de endpoints](supported-endpoints.md)
8. [API pública](public-api-policy.md)
9. [Estratégia de testes](testing.md)
10. [Troubleshooting](troubleshooting.md)
11. [Diagnóstico opcional com IA](ai-diagnostics.md)

## Requisitos

- .NET 10.
- Keycloak acessível pela aplicação.
- Client confidencial com Service Account, recomendado, ou usuário administrativo para Password Grant.
- Permissões mínimas do `realm-management` compatíveis com as operações executadas.

## Princípios de segurança

O SDK nunca registra tokens, Client Secrets, senhas, corpos HTTP ou query strings. OpenTelemetry e IA são opt-in no aplicativo consumidor; nenhum dado é enviado automaticamente para terceiros.
