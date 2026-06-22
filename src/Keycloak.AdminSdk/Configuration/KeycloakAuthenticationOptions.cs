namespace Keycloak.AdminSdk.Configuration;

/// <summary>Selects and configures the authentication flow used by the SDK.</summary>
public sealed class KeycloakAuthenticationOptions
{
    /// <summary>Gets or sets the selected authentication flow.</summary>
    public KeycloakAuthenticationFlow Flow { get; set; } = KeycloakAuthenticationFlow.ClientCredentials;

    /// <summary>Gets or sets Client Credentials flow settings.</summary>
    public ClientCredentialsOptions ClientCredentials { get; set; } = new();

    /// <summary>Gets or sets Password flow settings.</summary>
    public PasswordCredentialsOptions Password { get; set; } = new();

    /// <summary>Gets or sets how long before expiration a cached token is renewed.</summary>
    public TimeSpan TokenRefreshSkew { get; set; } = TimeSpan.FromSeconds(30);
}
