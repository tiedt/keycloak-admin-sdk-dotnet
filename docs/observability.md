# OpenTelemetry, logs, traces e métricas

## Modelo de integração

O SDK utiliza APIs nativas do .NET (`ILogger`, `ActivitySource` e `Meter`). Portanto, não exige OpenTelemetry no aplicativo. Se a aplicação não registrar um provider/exporter, não existe conexão externa.

Fontes públicas:

- ActivitySource: `Keycloak.AdminSdk`
- Meter: `Keycloak.AdminSdk`
- Logs: categoria `Keycloak.AdminSdk.*`

## Telemetria emitida

### Traces

- `keycloak.admin.request`
- `keycloak.authentication`

Tags relevantes:

- `http.request.method`
- `http.response.status_code`
- `url.path`, opcional e sem query string
- `exception.type`
- `keycloak.error.cause_hint`
- `keycloak.error.remediation_hint`

### Métricas

- `keycloak.admin.requests`
- `keycloak.admin.errors`
- `keycloak.admin.request.duration`, em milissegundos

As métricas evitam Realm, path e IDs para limitar cardinalidade.

### Logs estruturados

| EventId | Nome | Nível | Situação |
|---:|---|---|---|
| 2000 | `KeycloakRequestSucceeded` | Debug | Sucesso, somente quando habilitado. |
| 2001 | `KeycloakRequestFailed` | Warning | Falha HTTP 4xx. |
| 2002 | `KeycloakServerRequestFailed` | Error | Falha HTTP 5xx. |
| 2003 | `KeycloakRequestException` | Error | Exceção de rede, timeout ou cliente. |
| 2004 | `KeycloakDiagnosticSinkFailed` | Warning | Sink opcional falhou; operação principal continua. |
| 2101 | `KeycloakAuthenticationRejected` | Warning | Token endpoint rejeitou credenciais. |
| 2102 | `KeycloakAuthenticationException` | Error | Falha de transporte ao autenticar. |

## Instalar OpenTelemetry no aplicativo consumidor

```powershell
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Instrumentation.Http
```

## Configurar traces, métricas e logs

```csharp
using Keycloak.AdminSdk.Observability;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddSource(KeycloakTelemetry.ActivitySourceName)
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddMeter(KeycloakTelemetry.MeterName)
        .AddOtlpExporter());

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
    logging.AddOtlpExporter();
});
```

Configure o destino por ambiente:

```text
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
OTEL_SERVICE_NAME=orders-api
```

## Grafana

Use Grafana Alloy ou OpenTelemetry Collector como endpoint OTLP. O Collector pode encaminhar:

- Traces para Tempo.
- Logs para Loki.
- Métricas para Prometheus/Mimir.

O SDK não referencia Grafana diretamente. Isso permite trocar o backend sem recompilar o pacote.

## Produção

- Mantenha logs de sucesso desabilitados, salvo diagnóstico temporário.
- Use sampling para traces de sucesso; preserve traces de erro.
- Defina retenção e acesso no backend.
- Crie alertas por taxa de `keycloak.admin.errors`, status 401/403 e duração.
- Correlacione aplicação, Collector e Keycloak pelo TraceId.
