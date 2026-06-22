using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.Features.Sessions.Models;

public sealed record UserSessionResource
{
    public required string Id { get; init; }
    public KeycloakResourceId? UserId { get; init; }
    public string? Username { get; init; }
    public string? IpAddress { get; init; }
    public DateTimeOffset? StartedAtUtc { get; init; }
    public DateTimeOffset? LastAccessUtc { get; init; }
    public IReadOnlyDictionary<string, string> Clients { get; init; } = new Dictionary<string, string>();
}
