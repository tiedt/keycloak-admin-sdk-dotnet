namespace Keycloak.AdminSdk.Configuration;

/// <summary>Controls optional SDK telemetry emission.</summary>
public sealed class KeycloakObservabilityOptions
{
    /// <summary>Gets or sets whether traces, metrics, logs, and diagnostic events are emitted.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Gets or sets whether successful administrative requests are logged at Debug level.</summary>
    public bool LogSuccessfulRequests { get; set; }

    /// <summary>Gets or sets whether rule-based cause and remediation hints are emitted for failures.</summary>
    public bool EmitDiagnosticHints { get; set; } = true;

    /// <summary>Gets or sets whether sanitized request paths are included. Query strings are never included.</summary>
    public bool IncludeRequestPath { get; set; } = true;

    /// <summary>Gets or sets whether registered diagnostic sinks receive failure events.</summary>
    public bool PublishDiagnosticEvents { get; set; } = true;
}
