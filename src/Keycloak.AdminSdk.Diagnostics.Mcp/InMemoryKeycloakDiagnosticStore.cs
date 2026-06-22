using Keycloak.AdminSdk.Observability;
using Microsoft.Extensions.Options;

namespace Keycloak.AdminSdk.Diagnostics.Mcp;

internal sealed class InMemoryKeycloakDiagnosticStore : IKeycloakDiagnosticReader, IKeycloakDiagnosticSink
{
    private readonly object _gate = new();
    private readonly Queue<KeycloakDiagnosticEvent> _events = new();
    private readonly int _capacity;
    private readonly int _maximumQuerySize;

    public InMemoryKeycloakDiagnosticStore(IOptions<KeycloakMcpDiagnosticsOptions> options)
    {
        _capacity = options.Value.Capacity;
        _maximumQuerySize = options.Value.MaximumQuerySize;
        if (_capacity is < 1 or > 100_000) throw new InvalidOperationException("MCP diagnostic Capacity must be between 1 and 100000.");
        if (_maximumQuerySize is < 1 or > 1_000) throw new InvalidOperationException("MCP MaximumQuerySize must be between 1 and 1000.");
    }

    public ValueTask PublishAsync(KeycloakDiagnosticEvent diagnosticEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(diagnosticEvent);
        lock (_gate)
        {
            while (_events.Count >= _capacity) _events.Dequeue();
            _events.Enqueue(diagnosticEvent);
        }

        return ValueTask.CompletedTask;
    }

    public IReadOnlyList<KeycloakDiagnosticEvent> GetRecent(int limit, int? statusCode = null, string? traceId = null)
    {
        limit = Math.Clamp(limit, 1, _maximumQuerySize);
        lock (_gate)
        {
            return _events.Reverse()
                .Where(item => statusCode is null || item.HttpStatusCode == statusCode)
                .Where(item => string.IsNullOrWhiteSpace(traceId) || string.Equals(item.TraceId, traceId, StringComparison.Ordinal))
                .Take(limit)
                .ToArray();
        }
    }

    public KeycloakDiagnosticSummary GetSummary()
    {
        lock (_gate)
        {
            var snapshot = _events.ToArray();
            return new KeycloakDiagnosticSummary
            {
                TotalEvents = snapshot.Length,
                OldestEventUtc = snapshot.FirstOrDefault()?.TimestampUtc,
                NewestEventUtc = snapshot.LastOrDefault()?.TimestampUtc,
                EventsByStatusCode = snapshot.Where(item => item.HttpStatusCode is not null)
                    .GroupBy(item => item.HttpStatusCode!.Value)
                    .ToDictionary(group => group.Key, group => group.Count()),
                EventsByCause = snapshot.GroupBy(item => item.CauseHint, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal),
            };
        }
    }
}
