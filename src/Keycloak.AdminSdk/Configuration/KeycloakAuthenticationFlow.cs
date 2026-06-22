namespace Keycloak.AdminSdk.Configuration;

/// <summary>Authentication flows supported for administrative tokens.</summary>
public enum KeycloakAuthenticationFlow
{
    /// <summary>OAuth 2.0 Client Credentials flow using a service account.</summary>
    ClientCredentials = 0,

    /// <summary>OAuth 2.0 Resource Owner Password flow. Must be explicitly enabled.</summary>
    Password = 1,
}
