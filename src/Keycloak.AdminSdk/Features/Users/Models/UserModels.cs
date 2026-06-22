using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.Features.Users.Models;

public sealed record UserResource
{
    public required KeycloakResourceId Id { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool Enabled { get; init; }
    public bool EmailVerified { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Attributes { get; init; } = new Dictionary<string, IReadOnlyCollection<string>>();
}

public sealed record UserQuery
{
    public string? Search { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public bool Exact { get; init; }
    public int? First { get; init; }
    public int? Max { get; init; }
}

public sealed record CreateUserRequest(string Username)
{
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool Enabled { get; init; } = true;
    public bool EmailVerified { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Attributes { get; init; } = new Dictionary<string, IReadOnlyCollection<string>>();
}

public sealed record UpdateUserRequest
{
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool? Enabled { get; init; }
    public bool? EmailVerified { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>>? Attributes { get; init; }
}

public sealed record SetPasswordRequest(string Password)
{
    public bool Temporary { get; init; }
}
