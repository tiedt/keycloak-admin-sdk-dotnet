namespace Keycloak.AdminSdk.Diagnostics.Mcp;

/// <summary>Configures the bounded in-memory diagnostic buffer exposed through MCP.</summary>
public sealed class KeycloakMcpDiagnosticsOptions
{
    public int Capacity { get; set; } = 500;
    public int MaximumQuerySize { get; set; } = 100;
}
