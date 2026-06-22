namespace Keycloak.AdminSdk.Features.Events.Models;

public sealed record KeycloakEventResource
{
    public DateTimeOffset? TimestampUtc { get; init; }
    public string? Type { get; init; }
    public string? RealmId { get; init; }
    public string? ClientId { get; init; }
    public string? UserId { get; init; }
    public string? SessionId { get; init; }
    public string? IpAddress { get; init; }
    public string? Error { get; init; }
    public IReadOnlyDictionary<string, string> Details { get; init; } = new Dictionary<string, string>();
}

public sealed record KeycloakAdminEventResource
{
    public DateTimeOffset? TimestampUtc { get; init; }
    public string? RealmId { get; init; }
    public string? OperationType { get; init; }
    public string? ResourceType { get; init; }
    public string? ResourcePath { get; init; }
    public string? Error { get; init; }
}

public sealed record EventQuery
{
    public IReadOnlyCollection<string> Types { get; init; } = [];
    public string? ClientId { get; init; }
    public string? UserId { get; init; }
    public DateTimeOffset? DateFromUtc { get; init; }
    public DateTimeOffset? DateToUtc { get; init; }
    public int First { get; init; }
    public int Max { get; init; } = 100;
}

public sealed record AdminEventQuery
{
    public IReadOnlyCollection<string> OperationTypes { get; init; } = [];
    public IReadOnlyCollection<string> ResourceTypes { get; init; } = [];
    public string? ResourcePath { get; init; }
    public string? AuthClient { get; init; }
    public string? AuthUser { get; init; }
    public int First { get; init; }
    public int Max { get; init; } = 100;
}

public sealed record EventsConfiguration
{
    public bool EventsEnabled { get; init; }
    public bool AdminEventsEnabled { get; init; }
    public bool AdminEventsDetailsEnabled { get; init; }
    public long EventsExpirationSeconds { get; init; }
    public IReadOnlyCollection<string> EnabledEventTypes { get; init; } = [];
    public IReadOnlyCollection<string> EventListeners { get; init; } = [];
}
