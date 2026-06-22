using System.Net;
using System.Text;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Exceptions;
using Keycloak.AdminSdk.Features.Realms;
using Keycloak.AdminSdk.Features.Realms.Models;

namespace Keycloak.AdminSdk.UnitTests.Realms;

public sealed class KeycloakRealmServiceTests
{
    [Fact]
    public async Task GetAllMapsRealmRepresentations()
    {
        var httpClient = new StubHttpClient(JsonResponse(
            """[{"id":"realm-id","realm":"customers","enabled":true,"sslRequired":"external"}]"""));
        var service = new KeycloakRealmService(httpClient);

        var realms = await service.GetAllAsync();

        var realm = Assert.Single(realms);
        Assert.Equal("realm-id", realm.Id?.Value);
        Assert.Equal("customers", realm.Name.Value);
        Assert.True(realm.Enabled);
        Assert.Equal(SslRequirement.External, realm.SslRequired);
        Assert.Equal(HttpMethod.Get, httpClient.Requests[0].Method);
        Assert.Equal("admin/realms", httpClient.Requests[0].Uri);
    }

    [Fact]
    public async Task FindReturnsNullForMissingRealm()
    {
        var httpClient = new StubHttpClient(new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = new KeycloakRealmService(httpClient);

        var realm = await service.FindAsync("missing");

        Assert.Null(realm);
    }

    [Fact]
    public async Task CreatePostsTypedRepresentationAndReturnsPersistedRealm()
    {
        var httpClient = new StubHttpClient(
            new HttpResponseMessage(HttpStatusCode.Created),
            JsonResponse("""{"realm":"tenant-a","displayName":"Tenant A","enabled":true,"sslRequired":"all"}"""));
        var service = new KeycloakRealmService(httpClient);
        var request = new CreateRealmRequest("tenant-a")
        {
            DisplayName = "Tenant A",
            SslRequired = SslRequirement.All,
        };

        var realm = await service.CreateAsync(request);

        Assert.Equal("tenant-a", realm.Name.Value);
        Assert.Equal(SslRequirement.All, realm.SslRequired);
        Assert.Equal(HttpMethod.Post, httpClient.Requests[0].Method);
        Assert.Contains("\"realm\":\"tenant-a\"", httpClient.Requests[0].Body, StringComparison.Ordinal);
        Assert.Contains("\"sslRequired\":\"all\"", httpClient.Requests[0].Body, StringComparison.Ordinal);
        Assert.Equal(HttpMethod.Get, httpClient.Requests[1].Method);
    }

    [Fact]
    public async Task UpdateOmitsUnsetProperties()
    {
        var httpClient = new StubHttpClient(
            new HttpResponseMessage(HttpStatusCode.NoContent),
            JsonResponse("""{"realm":"tenant/a","enabled":false,"sslRequired":"external"}"""));
        var service = new KeycloakRealmService(httpClient);

        var realm = await service.UpdateAsync(
            "tenant/a",
            new UpdateRealmRequest { Enabled = false });

        Assert.False(realm.Enabled);
        Assert.Equal("admin/realms/tenant%2Fa", httpClient.Requests[0].Uri);
        Assert.Contains("\"enabled\":false", httpClient.Requests[0].Body, StringComparison.Ordinal);
        Assert.DoesNotContain("displayName", httpClient.Requests[0].Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteMapsConflictWithoutLeakingResponseBody()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent("sensitive-server-details"),
        };
        var service = new KeycloakRealmService(new StubHttpClient(response));

        var exception = await Assert.ThrowsAsync<KeycloakConflictException>(
            () => service.DeleteAsync("master"));

        Assert.DoesNotContain("sensitive-server-details", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EmptyUpdateIsRejectedBeforeSendingRequest()
    {
        var httpClient = new StubHttpClient();
        var service = new KeycloakRealmService(httpClient);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync("customers", new UpdateRealmRequest()));

        Assert.Empty(httpClient.Requests);
    }

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };

    private sealed class StubHttpClient(params HttpResponseMessage[] responses) : IKeycloakHttpClient
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);

        public List<RequestSnapshot> Requests { get; } = [];

        public async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new RequestSnapshot(
                request.Method,
                request.RequestUri?.OriginalString ?? string.Empty,
                body));

            return _responses.Dequeue();
        }
    }

    private sealed record RequestSnapshot(HttpMethod Method, string Uri, string Body);
}
