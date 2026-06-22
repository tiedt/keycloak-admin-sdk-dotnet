namespace Keycloak.AdminSdk.Configuration;

/// <summary>Configures authentication using a confidential client's service account.</summary>
public sealed class ClientCredentialsOptions
{
    /// <summary>Gets or sets the confidential client identifier.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Gets or sets the confidential client secret. This value must never be logged.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Gets or sets optional scopes requested when obtaining a token.</summary>
    public IReadOnlyCollection<string> Scopes { get; set; } = [];
}
