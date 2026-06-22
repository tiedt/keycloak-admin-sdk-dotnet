using Keycloak.AdminSdk.Diagnostics.Mcp;
using Keycloak.AdminSdk.Observability;
using Microsoft.Extensions.DependencyInjection;

namespace Keycloak.AdminSdk.Diagnostics.Mcp.UnitTests;

public sealed class DiagnosticStoreTests
{
    [Fact]
    public async Task RegistrationConnectsSdkSinkToMcpReader()
    {
        var services = new ServiceCollection();
        services.AddKeycloakAdminDiagnosticsMcp(options => options.Capacity = 10);
        using var provider = services.BuildServiceProvider();
        var sink = Assert.Single(provider.GetServices<IKeycloakDiagnosticSink>());
        var reader = provider.GetRequiredService<IKeycloakDiagnosticReader>();

        await sink.PublishAsync(CreateEvent(403, "trace-1"));

        var diagnostic = Assert.Single(reader.GetRecent(10));
        Assert.Equal(403, diagnostic.HttpStatusCode);
        Assert.Equal("trace-1", diagnostic.TraceId);
    }

    [Fact]
    public async Task BufferDropsOldestEventAtCapacity()
    {
        var services = new ServiceCollection();
        services.AddKeycloakAdminDiagnosticsMcp(options => options.Capacity = 2);
        using var provider = services.BuildServiceProvider();
        var sink = Assert.Single(provider.GetServices<IKeycloakDiagnosticSink>());
        var reader = provider.GetRequiredService<IKeycloakDiagnosticReader>();

        await sink.PublishAsync(CreateEvent(400, "first"));
        await sink.PublishAsync(CreateEvent(403, "second"));
        await sink.PublishAsync(CreateEvent(500, "third"));

        var events = reader.GetRecent(10);
        Assert.Equal(2, events.Count);
        Assert.DoesNotContain(events, item => item.TraceId == "first");
    }

    [Fact]
    public async Task ReaderFiltersAndSummarizesDiagnostics()
    {
        var services = new ServiceCollection();
        services.AddKeycloakAdminDiagnosticsMcp();
        using var provider = services.BuildServiceProvider();
        var sink = Assert.Single(provider.GetServices<IKeycloakDiagnosticSink>());
        var reader = provider.GetRequiredService<IKeycloakDiagnosticReader>();

        await sink.PublishAsync(CreateEvent(403, "trace-a"));
        await sink.PublishAsync(CreateEvent(500, "trace-b"));

        Assert.Single(reader.GetRecent(20, statusCode: 500));
        var summary = reader.GetSummary();
        Assert.Equal(2, summary.TotalEvents);
        Assert.Equal(1, summary.EventsByStatusCode[403]);
        Assert.Equal(1, summary.EventsByStatusCode[500]);
    }

    private static KeycloakDiagnosticEvent CreateEvent(int statusCode, string traceId) => new()
    {
        TimestampUtc = DateTimeOffset.UtcNow,
        Category = "test",
        HttpMethod = "GET",
        HttpStatusCode = statusCode,
        CauseHint = "cause",
        RemediationHint = "fix",
        TraceId = traceId,
    };
}
