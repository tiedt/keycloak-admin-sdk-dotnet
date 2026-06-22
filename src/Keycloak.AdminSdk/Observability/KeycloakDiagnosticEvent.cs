namespace Keycloak.AdminSdk.Observability;

/// <summary>A sanitized, structured SDK failure suitable for automated analysis.</summary>
public sealed record KeycloakDiagnosticEvent
{
    public required DateTimeOffset TimestampUtc { get; init; }
    public required string Category { get; init; }
    public required string HttpMethod { get; init; }
    public string? RequestPath { get; init; }
    public int? HttpStatusCode { get; init; }
    public string? ExceptionType { get; init; }
    public required string CauseHint { get; init; }
    public required string RemediationHint { get; init; }
    public string? TraceId { get; init; }
}
