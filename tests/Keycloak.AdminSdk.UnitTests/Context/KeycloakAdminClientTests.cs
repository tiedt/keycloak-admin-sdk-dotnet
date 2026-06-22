using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Configuration;
using Keycloak.AdminSdk.Context;
using Keycloak.AdminSdk.Features.Realms;
using Microsoft.Extensions.Options;

namespace Keycloak.AdminSdk.UnitTests.Context;

public sealed class KeycloakAdminClientTests
{
    [Fact]
    public void ConstructorCreatesConfiguredDefaultRealm()
    {
        var client = CreateClient("customers");

        Assert.NotNull(client.DefaultRealm);
        Assert.Equal("customers", client.DefaultRealm.Realm.Value);
    }

    [Fact]
    public void ConstructorWithoutDefaultRealmLeavesContextUnset()
    {
        var client = CreateClient(null);

        Assert.Null(client.DefaultRealm);
    }

    [Fact]
    public void ForRealmDoesNotChangeDefaultRealm()
    {
        var client = CreateClient("default-realm");

        var selectedRealm = client.ForRealm("other-realm");

        Assert.Equal("other-realm", selectedRealm.Realm.Value);
        Assert.Equal("default-realm", client.DefaultRealm?.Realm.Value);
    }

    private static KeycloakAdminClient CreateClient(string? defaultRealm) =>
        new(
            new KeycloakRealmContextFactory(new UnexpectedHttpClient()),
            new KeycloakRealmService(new UnexpectedHttpClient()),
            Options.Create(new KeycloakAdminOptions
            {
                DefaultRealm = defaultRealm,
            }));

    private sealed class UnexpectedHttpClient : IKeycloakHttpClient
    {
        public Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("No HTTP request was expected by this test.");
    }
}
