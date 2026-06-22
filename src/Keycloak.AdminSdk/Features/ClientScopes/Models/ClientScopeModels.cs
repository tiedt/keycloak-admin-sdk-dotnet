using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.Features.ClientScopes.Models;

public sealed record ClientScopeResource
{
    public required KeycloakResourceId Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string Protocol { get; init; } = "openid-connect";
    public IReadOnlyDictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();
}

public sealed record CreateClientScopeRequest(string Name)
{
    public string? Description { get; init; }
    public string Protocol { get; init; } = "openid-connect";
    public IReadOnlyDictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();
}

public sealed record UpdateClientScopeRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Protocol { get; init; }
    public IReadOnlyDictionary<string, string>? Attributes { get; init; }
}
