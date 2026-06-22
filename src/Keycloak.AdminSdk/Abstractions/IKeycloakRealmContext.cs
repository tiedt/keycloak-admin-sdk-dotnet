using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Abstractions.Clients;
using Keycloak.AdminSdk.Abstractions.ClientScopes;
using Keycloak.AdminSdk.Abstractions.Groups;
using Keycloak.AdminSdk.Abstractions.Roles;
using Keycloak.AdminSdk.Abstractions.Users;
using Keycloak.AdminSdk.Abstractions.Events;
using Keycloak.AdminSdk.Abstractions.IdentityProviders;
using Keycloak.AdminSdk.Abstractions.ProtocolMappers;
using Keycloak.AdminSdk.Abstractions.Sessions;

namespace Keycloak.AdminSdk.Abstractions;

/// <summary>
/// Represents an immutable administrative context bound to exactly one Keycloak realm.
/// </summary>
public interface IKeycloakRealmContext
{
    /// <summary>Gets the realm associated with this context.</summary>
    RealmName Realm { get; }

    IKeycloakUserService Users { get; }
    IKeycloakClientService Clients { get; }
    IKeycloakClientScopeService ClientScopes { get; }
    IKeycloakRoleService Roles { get; }
    IKeycloakGroupService Groups { get; }
    IKeycloakProtocolMapperService ProtocolMappers { get; }
    IKeycloakIdentityProviderService IdentityProviders { get; }
    IKeycloakSessionService Sessions { get; }
    IKeycloakEventService Events { get; }
}
