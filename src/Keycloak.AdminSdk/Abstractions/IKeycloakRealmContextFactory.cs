using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.Abstractions;

/// <summary>Creates immutable realm contexts without changing global SDK state.</summary>
public interface IKeycloakRealmContextFactory
{
    /// <summary>Creates or obtains a context for the specified realm.</summary>
    /// <param name="realm">The target realm.</param>
    IKeycloakRealmContext Create(RealmName realm);
}
