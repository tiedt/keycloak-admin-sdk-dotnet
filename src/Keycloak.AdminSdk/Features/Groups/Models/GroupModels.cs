using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.Features.Groups.Models;

public sealed record GroupResource
{
    public required KeycloakResourceId Id { get; init; }
    public required string Name { get; init; }
    public string? Path { get; init; }
    public IReadOnlyCollection<GroupResource> SubGroups { get; init; } = [];
}

public sealed record CreateGroupRequest(string Name);

public sealed record UpdateGroupRequest(string Name);
