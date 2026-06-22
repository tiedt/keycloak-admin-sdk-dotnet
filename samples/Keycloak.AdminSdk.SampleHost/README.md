# Keycloak.AdminSdk.SampleHost

Aplicação ASP.NET Core de referência para demonstrar uso seguro do SDK. Não é um gateway administrativo pronto para produção.

## Configurar secrets locais

```powershell
dotnet user-secrets init --project samples/Keycloak.AdminSdk.SampleHost
dotnet user-secrets set "Keycloak:Admin:Authentication:ClientCredentials:ClientSecret" "secret" --project samples/Keycloak.AdminSdk.SampleHost
```

Também configure no Keycloak:

- Client `keycloak-admin-sample` para validar os JWTs recebidos pelo Sample Host.
- Scope `keycloak.admin` para chamadas administrativas.
- Scope `keycloak.diagnostics` para MCP.
- Client administrativo confidencial `sample-host-admin` com Service Account e privilégio mínimo.

## Executar

```powershell
dotnet run --project samples/Keycloak.AdminSdk.SampleHost
```

Endpoints principais:

- `GET/POST /api/realms`
- `GET/POST/PUT/DELETE /api/realms/{realm}/users`
- `POST /api/realms/{realm}/provisioning/clients`
- `GET /health`
- `/mcp/keycloak`, somente quando habilitado e autorizado

## Variáveis OpenTelemetry

```text
Observability__Otlp__Enabled=true
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_SERVICE_NAME=keycloak-admin-sample
```
