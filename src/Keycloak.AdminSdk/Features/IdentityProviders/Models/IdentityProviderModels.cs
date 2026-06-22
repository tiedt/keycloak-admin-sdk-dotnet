namespace Keycloak.AdminSdk.Features.IdentityProviders.Models;

public sealed record IdentityProviderResource
{
    public required string Alias { get; init; }
    public required string ProviderId { get; init; }
    public string? DisplayName { get; init; }
    public bool Enabled { get; init; }
    public bool TrustEmail { get; init; }
    public bool StoreToken { get; init; }
    public bool LinkOnly { get; init; }
    public IReadOnlyDictionary<string, string> Config { get; init; } = new Dictionary<string, string>();
}

public sealed record CreateIdentityProviderRequest(string Alias, string ProviderId)
{
    public string? DisplayName { get; init; }
    public bool Enabled { get; init; } = true;
    public bool TrustEmail { get; init; }
    public bool StoreToken { get; init; }
    public bool LinkOnly { get; init; }
    public IReadOnlyDictionary<string, string> Config { get; init; } = new Dictionary<string, string>();
}

public sealed record UpdateIdentityProviderRequest
{
    public string? DisplayName { get; init; }
    public bool? Enabled { get; init; }
    public bool? TrustEmail { get; init; }
    public bool? StoreToken { get; init; }
    public bool? LinkOnly { get; init; }
    public IReadOnlyDictionary<string, string>? Config { get; init; }
}
