using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.Features.Roles.Models;

public sealed record RoleResource
{
    public KeycloakResourceId? Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool Composite { get; init; }
    public bool ClientRole { get; init; }
    public string? ContainerId { get; init; }
}

public sealed record CreateRoleRequest(string Name)
{
    public string? Description { get; init; }
}

public sealed record UpdateRoleRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
}
