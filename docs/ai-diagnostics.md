# Extensão opcional de diagnóstico com IA e MCP

Esta extensão não é necessária para utilizar o SDK e não deve ser o primeiro componente adotado. Estabilize autenticação, permissões, operações administrativas e telemetria convencional antes de expor uma interface MCP.

## Arquitetura recomendada

```text
Aplicação .NET
  ├─ ILogger / ActivitySource / Meter
  │      └─ OTLP → Collector/Alloy → Grafana, Azure Monitor etc.
  │
  └─ IKeycloakDiagnosticSink
         └─ buffer sanitizado → MCP read-only → agente de IA
```

OTLP exporta telemetria. MCP permite que uma IA consulte dados. Usar MCP como exporter criaria acoplamento, perda de backpressure e uma superfície de segurança desnecessária.

## Pacote MCP opcional

```powershell
dotnet add package Keycloak.AdminSdk.Diagnostics.Mcp --version 0.1.0-preview.1
```

Esse pacote depende do SDK C# oficial do Model Context Protocol. O pacote principal continua sem dependência MCP.

## Registrar servidor MCP HTTP

```csharp
builder.Services.AddKeycloakAdminDiagnosticsMcp(options =>
{
    options.Capacity = 500;
    options.MaximumQuerySize = 100;
});

var app = builder.Build();

app.MapMcp("/mcp/keycloak")
    .RequireAuthorization("McpDiagnostics");
```

Proteja o endpoint com autenticação forte, autorização dedicada, TLS e restrição de rede. Não exponha MCP anonimamente na internet.

## Tools disponíveis

### `keycloak_diagnostics_recent`

Retorna eventos recentes, com filtros opcionais por status HTTP e TraceId.

### `keycloak_diagnostics_summary`

Agrupa eventos por status e causa provável.

As duas tools são declaradas read-only, não destrutivas e sem acesso aberto a recursos externos.

## Conteúdo entregue à IA

Um evento contém somente:

- Timestamp UTC.
- Categoria.
- Método HTTP.
- Path sem query string, quando habilitado.
- Status HTTP.
- Tipo da exceção.
- Causa provável determinística.
- Sugestão de correção determinística.
- TraceId.

Não contém token, secret, password, payload HTTP ou query string.

## Limitações do buffer em memória

O adapter padrão guarda uma quantidade limitada de falhas por instância da aplicação. Reinícios removem o histórico e múltiplas réplicas possuem buffers independentes.

Em ambientes distribuídos, implemente `IKeycloakDiagnosticReader` consultando Loki, Tempo, Elasticsearch, Azure Monitor ou outro backend central. Registre essa implementação antes de `AddKeycloakAdminDiagnosticsMcp`; as tools MCP passarão a consultar o reader customizado.

## Fluxo sugerido para o agente de IA

1. Consultar `keycloak_diagnostics_summary`.
2. Selecionar status ou causa dominante.
3. Consultar `keycloak_diagnostics_recent` com filtro.
4. Usar TraceId para consultar traces e logs autorizados no backend.
5. Produzir resposta com causa, evidência, correção e nível de confiança.
6. Nunca executar alteração no Keycloak automaticamente.

Formato de resposta recomendado:

```json
{
  "cause": "Service Account sem manage-users",
  "evidence": ["HTTP 403", "operação POST em users", "TraceId ..."],
  "remediation": ["atribuir manage-users", "retestar com privilégio mínimo"],
  "confidence": 0.91,
  "requiresHumanApproval": true
}
```

## Guardrails

- A IA não deve receber credenciais.
- Recomendações não devem executar mudanças automaticamente.
- Dados vindos de logs devem ser tratados como conteúdo não confiável.
- Use allowlist de tools e campos.
- Audite acessos MCP.
- Imponha limites de taxa e tamanho de resposta.
