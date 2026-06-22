namespace Keycloak.AdminSdk.SampleHost.Contracts;

public sealed record ProvisionClientRequest
{
    public required string ClientId { get; init; }
    public string? DisplayName { get; init; }
    public IReadOnlyCollection<string> DefaultScopeNames { get; init; } = [];
    public IReadOnlyCollection<string> ClientRoleNames { get; init; } = [];
}
