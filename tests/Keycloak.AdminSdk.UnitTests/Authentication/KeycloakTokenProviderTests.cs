using System.Net;
using System.Text;
using Keycloak.AdminSdk.Authentication;
using Keycloak.AdminSdk.Configuration;
using Keycloak.AdminSdk.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;

namespace Keycloak.AdminSdk.UnitTests.Authentication;

public sealed class KeycloakTokenProviderTests
{
    [Fact]
    public async Task ConcurrentRequestsShareOneTokenRequest()
    {
        var handler = new RecordingHandler(_ => TokenResponse("token-1", 300));
        using var provider = CreateProvider(handler);

        var requests = Enumerable.Range(0, 20)
            .Select(_ => provider.GetAccessTokenAsync(CancellationToken.None).AsTask());
        var tokens = await Task.WhenAll(requests);

        Assert.All(tokens, token => Assert.Equal("token-1", token));
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task ClientCredentialsRequestUsesConfiguredRealmAndFormFields()
    {
        var handler = new RecordingHandler(_ => TokenResponse("token-1", 300));
        using var provider = CreateProvider(handler);

        await provider.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal(
            "https://identity.example/realms/admin-realm/protocol/openid-connect/token",
            handler.RequestUri?.AbsoluteUri);
        Assert.Contains("grant_type=client_credentials", handler.RequestBody, StringComparison.Ordinal);
        Assert.Contains("client_id=admin-client", handler.RequestBody, StringComparison.Ordinal);
        Assert.Contains("client_secret=secret", handler.RequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExpiredRefreshWindowObtainsNewToken()
    {
        var tokenNumber = 0;
        var timeProvider = new ManualTimeProvider(
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var handler = new RecordingHandler(_ =>
            TokenResponse($"token-{Interlocked.Increment(ref tokenNumber)}", 60));
        using var provider = CreateProvider(handler, timeProvider);

        var first = await provider.GetAccessTokenAsync(CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromSeconds(31));
        var second = await provider.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal("token-1", first);
        Assert.Equal("token-2", second);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task RejectedCredentialsDoNotExposeResponseBody()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("sensitive-provider-details"),
        });
        using var provider = CreateProvider(handler);

        var exception = await Assert.ThrowsAsync<KeycloakAuthenticationException>(
            () => provider.GetAccessTokenAsync(CancellationToken.None).AsTask());

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.DoesNotContain("sensitive-provider-details", exception.Message, StringComparison.Ordinal);
    }

    private static KeycloakTokenProvider CreateProvider(
        HttpMessageHandler handler,
        TimeProvider? timeProvider = null)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://identity.example/"),
        };
        var options = Options.Create(new KeycloakAdminOptions
        {
            ServerUrl = client.BaseAddress,
            AuthenticationRealm = "admin-realm",
            Authentication = new KeycloakAuthenticationOptions
            {
                ClientCredentials = new ClientCredentialsOptions
                {
                    ClientId = "admin-client",
                    ClientSecret = "secret",
                },
            },
        });

        return new KeycloakTokenProvider(
            new StubHttpClientFactory(client),
            options,
            timeProvider ?? TimeProvider.System,
            NullLogger<KeycloakTokenProvider>.Instance);
    }

    private static HttpResponseMessage TokenResponse(string accessToken, int expiresIn) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(
                $$"""{"access_token":"{{accessToken}}","expires_in":{{expiresIn}},"token_type":"Bearer"}""",
                Encoding.UTF8,
                "application/json"),
        };

    private sealed class StubHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        private int _requestCount;

        public int RequestCount => Volatile.Read(ref _requestCount);

        public Uri? RequestUri { get; private set; }

        public string RequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _requestCount);
            RequestUri = request.RequestUri;
            RequestBody = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return responseFactory(request);
        }
    }

    private sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow += duration;
    }
}
