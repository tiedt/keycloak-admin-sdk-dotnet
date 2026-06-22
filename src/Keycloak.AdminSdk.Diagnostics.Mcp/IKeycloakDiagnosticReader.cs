using Keycloak.AdminSdk.Observability;

namespace Keycloak.AdminSdk.Diagnostics.Mcp;

/// <summary>Reads sanitized diagnostics from memory or an external observability backend.</summary>
public interface IKeycloakDiagnosticReader
{
    IReadOnlyList<KeycloakDiagnosticEvent> GetRecent(int limit, int? statusCode = null, string? traceId = null);
    KeycloakDiagnosticSummary GetSummary();
}
