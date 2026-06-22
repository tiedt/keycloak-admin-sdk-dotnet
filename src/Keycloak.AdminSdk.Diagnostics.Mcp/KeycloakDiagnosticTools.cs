using System.ComponentModel;
using Keycloak.AdminSdk.Observability;
using ModelContextProtocol.Server;

namespace Keycloak.AdminSdk.Diagnostics.Mcp;

/// <summary>Read-only MCP tools for sanitized Keycloak SDK diagnostics.</summary>
[McpServerToolType]
public sealed class KeycloakDiagnosticTools(IKeycloakDiagnosticReader reader)
{
    [McpServerTool(
        Name = "keycloak_diagnostics_recent",
        Title = "Recent Keycloak diagnostics",
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Returns recent sanitized Keycloak.AdminSdk failures. It never returns tokens, secrets, payload bodies, or query strings.")]
    public IReadOnlyList<KeycloakDiagnosticEvent> GetRecent(
        [Description("Number of events from 1 up to the configured maximum.")] int limit = 20,
        [Description("Optional HTTP status filter, for example 403 or 500.")] int? statusCode = null,
        [Description("Optional exact OpenTelemetry TraceId.")] string? traceId = null) =>
        reader.GetRecent(limit, statusCode, traceId);

    [McpServerTool(
        Name = "keycloak_diagnostics_summary",
        Title = "Keycloak diagnostic summary",
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Summarizes buffered Keycloak SDK failures by HTTP status and likely cause.")]
    public KeycloakDiagnosticSummary GetSummary() => reader.GetSummary();
}
