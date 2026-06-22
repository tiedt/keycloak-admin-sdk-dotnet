using System.Text.Json.Serialization;

namespace Keycloak.AdminSdk.Internal.Representations;

internal sealed class UserRepresentation
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("username")] public string? Username { get; init; }
    [JsonPropertyName("email")] public string? Email { get; init; }
    [JsonPropertyName("firstName")] public string? FirstName { get; init; }
    [JsonPropertyName("lastName")] public string? LastName { get; init; }
    [JsonPropertyName("enabled")] public bool? Enabled { get; init; }
    [JsonPropertyName("emailVerified")] public bool? EmailVerified { get; init; }
    [JsonPropertyName("attributes")] public Dictionary<string, List<string>>? Attributes { get; init; }
}

internal sealed class CredentialRepresentation
{
    [JsonPropertyName("type")] public string Type { get; init; } = "password";
    [JsonPropertyName("value")] public required string Value { get; init; }
    [JsonPropertyName("temporary")] public bool Temporary { get; init; }
}

internal sealed class ClientRepresentation
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("clientId")] public string? ClientId { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }
    [JsonPropertyName("secret")] public string? Secret { get; init; }
    [JsonPropertyName("enabled")] public bool? Enabled { get; init; }
    [JsonPropertyName("publicClient")] public bool? PublicClient { get; init; }
    [JsonPropertyName("serviceAccountsEnabled")] public bool? ServiceAccountsEnabled { get; init; }
    [JsonPropertyName("standardFlowEnabled")] public bool? StandardFlowEnabled { get; init; }
    [JsonPropertyName("directAccessGrantsEnabled")] public bool? DirectAccessGrantsEnabled { get; init; }
    [JsonPropertyName("protocol")] public string? Protocol { get; init; }
    [JsonPropertyName("redirectUris")] public List<string>? RedirectUris { get; init; }
    [JsonPropertyName("webOrigins")] public List<string>? WebOrigins { get; init; }
}

internal sealed class ClientScopeRepresentation
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }
    [JsonPropertyName("protocol")] public string? Protocol { get; init; }
    [JsonPropertyName("attributes")] public Dictionary<string, string>? Attributes { get; init; }
}

internal sealed class RoleRepresentation
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }
    [JsonPropertyName("composite")] public bool? Composite { get; init; }
    [JsonPropertyName("clientRole")] public bool? ClientRole { get; init; }
    [JsonPropertyName("containerId")] public string? ContainerId { get; init; }
}

internal sealed class GroupRepresentation
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("path")] public string? Path { get; init; }
    [JsonPropertyName("subGroups")] public List<GroupRepresentation>? SubGroups { get; init; }
}
