using Keycloak.AdminSdk.Abstractions;
using Keycloak.AdminSdk.Abstractions.Realms;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Configuration;
using Microsoft.Extensions.Options;

namespace Keycloak.AdminSdk.Context;

internal sealed class KeycloakAdminClient : IKeycloakAdminClient
{
    private readonly IKeycloakRealmContextFactory _realmContextFactory;

    public KeycloakAdminClient(
        IKeycloakRealmContextFactory realmContextFactory,
        IKeycloakRealmService realmService,
        IOptions<KeycloakAdminOptions> options)
    {
        _realmContextFactory = realmContextFactory;
        Realms = realmService;
        DefaultRealm = string.IsNullOrWhiteSpace(options.Value.DefaultRealm)
            ? null
            : realmContextFactory.Create(options.Value.DefaultRealm);
    }

    public IKeycloakRealmService Realms { get; }

    public IKeycloakRealmContext? DefaultRealm { get; }

    public IKeycloakRealmContext ForRealm(RealmName realm) =>
        _realmContextFactory.Create(realm);
}
