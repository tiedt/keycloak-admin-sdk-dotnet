using System.Text.Json;
using System.Text.Json.Serialization;

namespace Keycloak.AdminSdk.Internal.Serialization;

internal static class KeycloakJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };
}
