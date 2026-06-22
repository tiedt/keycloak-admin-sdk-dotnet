using System.Text.Json.Serialization;

namespace Keycloak.AdminSdk.Internal.Representations;

internal sealed class ProtocolMapperRepresentation
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("protocol")] public string? Protocol { get; init; }
    [JsonPropertyName("protocolMapper")] public string? ProtocolMapper { get; init; }
    [JsonPropertyName("consentRequired")] public bool? ConsentRequired { get; init; }
    [JsonPropertyName("consentText")] public string? ConsentText { get; init; }
    [JsonPropertyName("config")] public Dictionary<string, string>? Config { get; init; }
}

internal sealed class IdentityProviderRepresentation
{
    [JsonPropertyName("alias")] public string? Alias { get; init; }
    [JsonPropertyName("providerId")] public string? ProviderId { get; init; }
    [JsonPropertyName("displayName")] public string? DisplayName { get; init; }
    [JsonPropertyName("enabled")] public bool? Enabled { get; init; }
    [JsonPropertyName("trustEmail")] public bool? TrustEmail { get; init; }
    [JsonPropertyName("storeToken")] public bool? StoreToken { get; init; }
    [JsonPropertyName("linkOnly")] public bool? LinkOnly { get; init; }
    [JsonPropertyName("config")] public Dictionary<string, string>? Config { get; init; }
}

internal sealed class UserSessionRepresentation
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("userId")] public string? UserId { get; init; }
    [JsonPropertyName("username")] public string? Username { get; init; }
    [JsonPropertyName("ipAddress")] public string? IpAddress { get; init; }
    [JsonPropertyName("start")] public long? Start { get; init; }
    [JsonPropertyName("lastAccess")] public long? LastAccess { get; init; }
    [JsonPropertyName("clients")] public Dictionary<string, string>? Clients { get; init; }
}

internal sealed class EventRepresentation
{
    [JsonPropertyName("time")] public long? Time { get; init; }
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("realmId")] public string? RealmId { get; init; }
    [JsonPropertyName("clientId")] public string? ClientId { get; init; }
    [JsonPropertyName("userId")] public string? UserId { get; init; }
    [JsonPropertyName("sessionId")] public string? SessionId { get; init; }
    [JsonPropertyName("ipAddress")] public string? IpAddress { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }
    [JsonPropertyName("details")] public Dictionary<string, string>? Details { get; init; }
}

internal sealed class AdminEventRepresentation
{
    [JsonPropertyName("time")] public long? Time { get; init; }
    [JsonPropertyName("realmId")] public string? RealmId { get; init; }
    [JsonPropertyName("operationType")] public string? OperationType { get; init; }
    [JsonPropertyName("resourceType")] public string? ResourceType { get; init; }
    [JsonPropertyName("resourcePath")] public string? ResourcePath { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }
}

internal sealed class EventsConfigRepresentation
{
    [JsonPropertyName("eventsEnabled")] public bool? EventsEnabled { get; init; }
    [JsonPropertyName("adminEventsEnabled")] public bool? AdminEventsEnabled { get; init; }
    [JsonPropertyName("adminEventsDetailsEnabled")] public bool? AdminEventsDetailsEnabled { get; init; }
    [JsonPropertyName("eventsExpiration")] public long? EventsExpiration { get; init; }
    [JsonPropertyName("enabledEventTypes")] public List<string>? EnabledEventTypes { get; init; }
    [JsonPropertyName("eventsListeners")] public List<string>? EventsListeners { get; init; }
}
