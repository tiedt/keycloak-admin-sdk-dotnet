using System.Collections.Concurrent;
using Keycloak.AdminSdk.Abstractions;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.Context;

internal sealed class KeycloakRealmContextFactory(IKeycloakHttpClient httpClient) : IKeycloakRealmContextFactory
{
    private readonly ConcurrentDictionary<RealmName, IKeycloakRealmContext> _contexts = new();

    public IKeycloakRealmContext Create(RealmName realm)
    {
        if (string.IsNullOrWhiteSpace(realm.Value))
        {
            throw new ArgumentException("A realm name is required.", nameof(realm));
        }

        return _contexts.GetOrAdd(realm, realmName => new KeycloakRealmContext(realmName, httpClient));
    }
}
