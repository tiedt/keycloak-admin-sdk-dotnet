using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.Features.ProtocolMappers.Models;

public sealed record ProtocolMapperResource
{
    public required KeycloakResourceId Id { get; init; }
    public required string Name { get; init; }
    public required string Protocol { get; init; }
    public required string ProtocolMapper { get; init; }
    public bool ConsentRequired { get; init; }
    public string? ConsentText { get; init; }
    public IReadOnlyDictionary<string, string> Config { get; init; } = new Dictionary<string, string>();
}

public sealed record CreateProtocolMapperRequest(string Name, string ProtocolMapper)
{
    public string Protocol { get; init; } = "openid-connect";
    public bool ConsentRequired { get; init; }
    public string? ConsentText { get; init; }
    public IReadOnlyDictionary<string, string> Config { get; init; } = new Dictionary<string, string>();
}

public sealed record UpdateProtocolMapperRequest
{
    public string? Name { get; init; }
    public string? Protocol { get; init; }
    public string? ProtocolMapper { get; init; }
    public bool? ConsentRequired { get; init; }
    public string? ConsentText { get; init; }
    public IReadOnlyDictionary<string, string>? Config { get; init; }
}
