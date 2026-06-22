namespace Keycloak.AdminSdk.Features.Realms.Models;

/// <summary>Defines when Keycloak requires HTTPS for a realm.</summary>
public enum SslRequirement
{
    /// <summary>The server returned a value unknown to this SDK version.</summary>
    Unknown = 0,

    /// <summary>HTTPS is not required.</summary>
    None = 1,

    /// <summary>HTTPS is required for external requests.</summary>
    External = 2,

    /// <summary>HTTPS is required for every request.</summary>
    All = 3,
}
