namespace Keycloak.AdminSdk.Configuration;

/// <summary>Configures resilience behavior for calls to Keycloak.</summary>
public sealed class KeycloakResilienceOptions
{
    /// <summary>Gets or sets the maximum number of retry attempts.</summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Gets or sets the base delay used by retry backoff.</summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>Gets or sets the timeout applied to an individual attempt.</summary>
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Gets or sets the total timeout for a complete HTTP operation.</summary>
    public TimeSpan TotalRequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
