namespace Keycloak.AdminSdk.Diagnostics.Mcp;

/// <summary>Aggregated diagnostics exposed to MCP clients.</summary>
public sealed record KeycloakDiagnosticSummary
{
    public required int TotalEvents { get; init; }
    public DateTimeOffset? OldestEventUtc { get; init; }
    public DateTimeOffset? NewestEventUtc { get; init; }
    public required IReadOnlyDictionary<int, int> EventsByStatusCode { get; init; }
    public required IReadOnlyDictionary<string, int> EventsByCause { get; init; }
}
