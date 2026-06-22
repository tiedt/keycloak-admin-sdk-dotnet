namespace Keycloak.AdminSdk.Observability;

/// <summary>Receives sanitized diagnostic events for queues, incident systems, or AI analysis.</summary>
public interface IKeycloakDiagnosticSink
{
    /// <summary>Publishes one diagnostic event. Implementations must not throw intentionally.</summary>
    ValueTask PublishAsync(KeycloakDiagnosticEvent diagnosticEvent, CancellationToken cancellationToken = default);
}
