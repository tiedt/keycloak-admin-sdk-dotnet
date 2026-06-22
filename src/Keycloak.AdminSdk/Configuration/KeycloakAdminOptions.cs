namespace Keycloak.AdminSdk.Configuration;

/// <summary>Configures access to the Keycloak Admin REST API.</summary>
public sealed class KeycloakAdminOptions
{
    /// <summary>The conventional configuration section name.</summary>
    public const string SectionName = "Keycloak:Admin";

    /// <summary>Gets or sets the Keycloak server base address.</summary>
    public Uri? ServerUrl { get; set; }

    /// <summary>Gets or sets the realm used by services resolved directly from DI.</summary>
    public string? DefaultRealm { get; set; }

    /// <summary>Gets or sets the realm whose token endpoint authenticates the SDK.</summary>
    public string AuthenticationRealm { get; set; } = "master";

    /// <summary>Gets or sets administrative authentication options.</summary>
    public KeycloakAuthenticationOptions Authentication { get; set; } = new();

    /// <summary>Gets or sets HTTP resilience options.</summary>
    public KeycloakResilienceOptions Resilience { get; set; } = new();

    /// <summary>Gets or sets structured logging, tracing, metrics, and diagnostics options.</summary>
    public KeycloakObservabilityOptions Observability { get; set; } = new();
}
