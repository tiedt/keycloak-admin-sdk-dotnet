using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Abstractions.Realms;

namespace Keycloak.AdminSdk.Abstractions;

/// <summary>Provides the main entry point for realm-scoped Keycloak administration.</summary>
public interface IKeycloakAdminClient
{
    /// <summary>Gets the global realm administration service.</summary>
    IKeycloakRealmService Realms { get; }

    /// <summary>
    /// Gets the configured default realm context, or <see langword="null"/> when no default realm was configured.
    /// </summary>
    IKeycloakRealmContext? DefaultRealm { get; }

    /// <summary>Gets an immutable administrative context for a realm.</summary>
    /// <param name="realm">The target realm.</param>
    IKeycloakRealmContext ForRealm(RealmName realm);
}
