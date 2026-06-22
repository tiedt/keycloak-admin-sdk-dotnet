using System.Net;
using Keycloak.AdminSdk.Authentication;

namespace Keycloak.AdminSdk.UnitTests.Authentication;

public sealed class AuthenticationDelegatingHandlerTests
{
    [Fact]
    public async Task SendAddsBearerToken()
    {
        var tokenProvider = new StubTokenProvider("access-token");
        var terminalHandler = new TerminalHandler(HttpStatusCode.OK);
        using var handler = new AuthenticationDelegatingHandler(tokenProvider)
        {
            InnerHandler = terminalHandler,
        };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://identity.example/admin/realms");

        using var response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal("Bearer", terminalHandler.AuthorizationScheme);
        Assert.Equal("access-token", terminalHandler.AuthorizationParameter);
        Assert.False(tokenProvider.WasInvalidated);
    }

    [Fact]
    public async Task UnauthorizedResponseInvalidatesCachedToken()
    {
        var tokenProvider = new StubTokenProvider("access-token");
        using var handler = new AuthenticationDelegatingHandler(tokenProvider)
        {
            InnerHandler = new TerminalHandler(HttpStatusCode.Unauthorized),
        };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://identity.example/admin/realms");

        using var response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.True(tokenProvider.WasInvalidated);
    }

    private sealed class StubTokenProvider(string token) : IKeycloakTokenProvider
    {
        public bool WasInvalidated { get; private set; }

        public ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken) =>
            ValueTask.FromResult(token);

        public void Invalidate() => WasInvalidated = true;
    }

    private sealed class TerminalHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public string? AuthorizationScheme { get; private set; }

        public string? AuthorizationParameter { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            return Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
