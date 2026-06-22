using Keycloak.AdminSdk.Abstractions;
using Keycloak.AdminSdk.Abstractions.Clients;
using Keycloak.AdminSdk.Abstractions.ClientScopes;
using Keycloak.AdminSdk.Abstractions.Groups;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Abstractions.Roles;
using Keycloak.AdminSdk.Abstractions.Users;
using Keycloak.AdminSdk.Abstractions.Events;
using Keycloak.AdminSdk.Abstractions.IdentityProviders;
using Keycloak.AdminSdk.Abstractions.ProtocolMappers;
using Keycloak.AdminSdk.Abstractions.Sessions;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Clients;
using Keycloak.AdminSdk.Features.ClientScopes;
using Keycloak.AdminSdk.Features.Groups;
using Keycloak.AdminSdk.Features.Roles;
using Keycloak.AdminSdk.Features.Users;
using Keycloak.AdminSdk.Features.Events;
using Keycloak.AdminSdk.Features.IdentityProviders;
using Keycloak.AdminSdk.Features.ProtocolMappers;
using Keycloak.AdminSdk.Features.Sessions;

namespace Keycloak.AdminSdk.Context;

internal sealed class KeycloakRealmContext : IKeycloakRealmContext
{
    public KeycloakRealmContext(RealmName realm, IKeycloakHttpClient httpClient)
    {
        Realm = realm;
        Users = new KeycloakUserService(httpClient, realm);
        Roles = new KeycloakRoleService(httpClient, realm);
        Clients = new KeycloakClientService(httpClient, realm, Roles);
        ClientScopes = new KeycloakClientScopeService(httpClient, realm);
        Groups = new KeycloakGroupService(httpClient, realm);
        ProtocolMappers = new KeycloakProtocolMapperService(httpClient, realm);
        IdentityProviders = new KeycloakIdentityProviderService(httpClient, realm);
        Sessions = new KeycloakSessionService(httpClient, realm);
        Events = new KeycloakEventService(httpClient, realm);
    }

    public RealmName Realm { get; }
    public IKeycloakUserService Users { get; }
    public IKeycloakClientService Clients { get; }
    public IKeycloakClientScopeService ClientScopes { get; }
    public IKeycloakRoleService Roles { get; }
    public IKeycloakGroupService Groups { get; }
    public IKeycloakProtocolMapperService ProtocolMappers { get; }
    public IKeycloakIdentityProviderService IdentityProviders { get; }
    public IKeycloakSessionService Sessions { get; }
    public IKeycloakEventService Events { get; }
}
