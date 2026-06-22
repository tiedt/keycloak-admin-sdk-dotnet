using System.Diagnostics;
using System.Net;
using Keycloak.AdminSdk.Configuration;
using Keycloak.AdminSdk.Http;
using Keycloak.AdminSdk.Observability;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Keycloak.AdminSdk.UnitTests.Observability;

public sealed class KeycloakHttpClientObservabilityTests
{
    [Fact]
    public async Task FailedResponsePublishesSanitizedDiagnosticWithHints()
    {
        var sink = new CollectingSink();
        var client = CreateClient(HttpStatusCode.Forbidden, [sink]);
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "admin/realms/customers/users?username=alice@example.test");

        using var response = await client.SendAsync(request);

        var diagnostic = Assert.Single(sink.Events);
        Assert.Equal(403, diagnostic.HttpStatusCode);
        Assert.Equal("admin/realms/customers/users", diagnostic.RequestPath);
        Assert.DoesNotContain("alice", diagnostic.RequestPath, StringComparison.Ordinal);
        Assert.Contains("permission", diagnostic.CauseHint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("realm-management", diagnostic.RemediationHint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DisabledObservabilityDoesNotPublishDiagnostics()
    {
        var sink = new CollectingSink();
        var client = CreateClient(HttpStatusCode.InternalServerError, [sink], enabled: false);
        using var request = new HttpRequestMessage(HttpMethod.Get, "admin/realms");

        using var response = await client.SendAsync(request);

        Assert.Empty(sink.Events);
    }

    [Fact]
    public async Task FailedResponseCreatesOpenTelemetryCompatibleActivity()
    {
        Activity? stoppedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == KeycloakTelemetry.ActivitySourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity => stoppedActivity = activity,
        };
        ActivitySource.AddActivityListener(listener);
        var client = CreateClient(HttpStatusCode.NotFound, []);
        using var request = new HttpRequestMessage(HttpMethod.Get, "admin/realms/missing");

        using var response = await client.SendAsync(request);

        Assert.NotNull(stoppedActivity);
        Assert.Equal(ActivityStatusCode.Error, stoppedActivity.Status);
        Assert.Equal("GET", stoppedActivity.GetTagItem("http.request.method"));
        Assert.Equal(404, stoppedActivity.GetTagItem("http.response.status_code"));
    }

    private static KeycloakHttpClient CreateClient(
        HttpStatusCode statusCode,
        IReadOnlyCollection<IKeycloakDiagnosticSink> sinks,
        bool enabled = true)
    {
        var httpClient = new HttpClient(new ResponseHandler(statusCode))
        {
            BaseAddress = new Uri("https://identity.example/"),
        };
        var options = Options.Create(new KeycloakAdminOptions
        {
            Observability = new KeycloakObservabilityOptions { Enabled = enabled },
        });
        return new KeycloakHttpClient(
            new StubHttpClientFactory(httpClient),
            options,
            sinks,
            NullLogger<KeycloakHttpClient>.Instance,
            TimeProvider.System);
    }

    private sealed class CollectingSink : IKeycloakDiagnosticSink
    {
        public List<KeycloakDiagnosticEvent> Events { get; } = [];

        public ValueTask PublishAsync(KeycloakDiagnosticEvent diagnosticEvent, CancellationToken cancellationToken = default)
        {
            Events.Add(diagnosticEvent);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class StubHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class ResponseHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode));
    }
}
