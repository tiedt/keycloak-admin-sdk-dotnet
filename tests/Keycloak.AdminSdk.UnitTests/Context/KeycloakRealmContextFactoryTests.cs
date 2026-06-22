using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Context;
using Keycloak.AdminSdk.Abstractions.Http;

namespace Keycloak.AdminSdk.UnitTests.Context;

public sealed class KeycloakRealmContextFactoryTests
{
    [Fact]
    public void CreateBindsContextToRequestedRealm()
    {
        var factory = CreateFactory();

        var context = factory.Create("customers");

        Assert.Equal(new RealmName("customers"), context.Realm);
    }

    [Fact]
    public void CreateForDifferentRealmsProducesIsolatedContexts()
    {
        var factory = CreateFactory();

        var first = factory.Create("company-a");
        var second = factory.Create("company-b");

        Assert.Equal("company-a", first.Realm.Value);
        Assert.Equal("company-b", second.Realm.Value);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void CreateRejectsDefaultRealmValue()
    {
        var factory = CreateFactory();

        Assert.Throws<ArgumentException>(() => factory.Create(default));
    }

    [Fact]
    public async Task CreateIsSafeForConcurrentUse()
    {
        var factory = CreateFactory();

        var tasks = Enumerable.Range(0, 50)
            .Select(_ => Task.Run(() => factory.Create("customers")));
        var contexts = await Task.WhenAll(tasks);

        Assert.All(contexts, context => Assert.Same(contexts[0], context));
    }

    private static KeycloakRealmContextFactory CreateFactory() => new(new UnexpectedHttpClient());

    private sealed class UnexpectedHttpClient : IKeycloakHttpClient
    {
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("No HTTP request was expected by this test.");
    }
}
