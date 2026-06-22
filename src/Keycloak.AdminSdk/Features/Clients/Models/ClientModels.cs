using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.Features.Clients.Models;

public sealed record ClientResource
{
    public required KeycloakResourceId Id { get; init; }
    public required string ClientId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool Enabled { get; init; }
    public bool PublicClient { get; init; }
    public bool ServiceAccountsEnabled { get; init; }
    public bool StandardFlowEnabled { get; init; }
    public bool DirectAccessGrantsEnabled { get; init; }
    public string Protocol { get; init; } = "openid-connect";
    public IReadOnlyCollection<string> RedirectUris { get; init; } = [];
    public IReadOnlyCollection<string> WebOrigins { get; init; } = [];
}

public sealed record CreateClientRequest(string ClientId)
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Secret { get; init; }
    public bool Enabled { get; init; } = true;
    public bool PublicClient { get; init; }
    public bool ServiceAccountsEnabled { get; init; }
    public bool StandardFlowEnabled { get; init; } = true;
    public bool DirectAccessGrantsEnabled { get; init; } = true;
    public string Protocol { get; init; } = "openid-connect";
    public IReadOnlyCollection<string> RedirectUris { get; init; } = [];
    public IReadOnlyCollection<string> WebOrigins { get; init; } = [];
}

public sealed record UpdateClientRequest
{
    public string? ClientId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? Enabled { get; init; }
    public bool? PublicClient { get; init; }
    public bool? ServiceAccountsEnabled { get; init; }
    public bool? StandardFlowEnabled { get; init; }
    public bool? DirectAccessGrantsEnabled { get; init; }
    public IReadOnlyCollection<string>? RedirectUris { get; init; }
    public IReadOnlyCollection<string>? WebOrigins { get; init; }
}
