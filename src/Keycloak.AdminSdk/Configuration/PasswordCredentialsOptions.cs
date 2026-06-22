namespace Keycloak.AdminSdk.Configuration;

/// <summary>Configures password-based authentication for constrained environments.</summary>
public sealed class PasswordCredentialsOptions
{
    /// <summary>Gets or sets the client identifier.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional confidential client secret.</summary>
    public string? ClientSecret { get; set; }

    /// <summary>Gets or sets the administrative username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the administrative password. This value must never be logged.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets optional scopes requested when obtaining a token.</summary>
    public IReadOnlyCollection<string> Scopes { get; set; } = [];

    /// <summary>Gets or sets whether the less-preferred password flow is explicitly permitted.</summary>
    public bool AllowPasswordGrant { get; set; }
}
