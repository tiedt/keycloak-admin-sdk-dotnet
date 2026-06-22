# Keycloak.AdminSdk

[![CI](https://github.com/tiedt/keycloak-admin-sdk-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/tiedt/keycloak-admin-sdk-dotnet/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/vpre/Keycloak.AdminSdk.svg)](https://www.nuget.org/packages/Keycloak.AdminSdk)

SDK .NET 10 fortemente tipado para integração e administração do Keycloak pela Admin REST API.

> `0.1.0-preview.1`: preview para validação pública. Consulte a matriz de compatibilidade antes de utilizar em produção.

## Principais recursos

- Realms, Users, Clients, Client Scopes, Roles, Groups, Protocol Mappers, Identity Providers, Sessions e Events.
- API baseada em interfaces e integrada à DI nativa do .NET.
- Contextos imutáveis para múltiplos Realms.
- Client Credentials recomendado e Password Grant opcional.
- Cache e renovação concorrente de tokens.
- `IHttpClientFactory`, retry seguro, jitter, circuit breaker e timeouts.
- Provisionamento fluído com compensação de falhas.
- OpenTelemetry opcional: logs estruturados, traces e métricas.

## Instalação

```powershell
dotnet add package Keycloak.AdminSdk --version 0.1.0-preview.1
```

## Configuração mínima

```csharp
builder.Services.AddKeycloakAdmin(options =>
{
    options.ServerUrl = new Uri("https://identity.example.com");
    options.DefaultRealm = "customers";
    options.AuthenticationRealm = "master";
    options.Authentication.ClientCredentials.ClientId = "admin-service";
    options.Authentication.ClientCredentials.ClientSecret = secret;
});
```

## Uso

```csharp
var tenant = keycloak.ForRealm("tenant-a");

var user = await tenant.Users.CreateAsync(
    new CreateUserRequest("alice") { Email = "alice@example.com" });

var group = await tenant.Groups.CreateAsync(new CreateGroupRequest("operators"));
await tenant.Groups.AddUserAsync(group.Id, user.Id);
```

## Documentação

- [Visão geral](docs/index.md)
- [Instalação passo a passo](docs/getting-started.md)
- [Configuração](docs/configuration.md)
- [Módulos e exemplos](docs/modules.md)
- [OpenTelemetry e Grafana](docs/observability.md)
- [Segurança](docs/security.md)
- [Compatibilidade e versionamento](docs/compatibility.md)
- [Cobertura de endpoints](docs/supported-endpoints.md)
- [Política da API pública](docs/public-api-policy.md)
- [Estratégia e matriz de testes](docs/testing.md)
- [Troubleshooting](docs/troubleshooting.md)

## Extensão opcional de diagnóstico

Depois que o SDK principal estiver integrado e confiável, o pacote separado `Keycloak.AdminSdk.Diagnostics.Mcp` pode expor diagnósticos sanitizados e read-only para agentes autorizados. Ele é desabilitado por padrão e não participa do transporte de telemetria.

## Arquitetura de observabilidade

OTLP é o transporte de logs, traces e métricas para Grafana ou outro backend. MCP é uma interface opcional para uma IA consultar diagnósticos sanitizados; não é utilizado como exporter.

```text
Aplicação → OpenTelemetry/OTLP → Collector → Grafana
         └→ Diagnostic Sink → MCP read-only → IA
```

Nenhum exporter, servidor MCP ou provedor de IA é ativado automaticamente pelo pacote principal. Consulte o [threat model](docs/threat-model-observability-mcp.md) antes de habilitar MCP.

## Testes locais

```powershell
docker-compose up -d
dotnet test Keycloak.AdminSdk.sln --configuration Release
dotnet test tests/Keycloak.AdminSdk.IntegrationTests `
  --settings tests/Keycloak.AdminSdk.IntegrationTests/local.runsettings
```

## Licença

MIT.
