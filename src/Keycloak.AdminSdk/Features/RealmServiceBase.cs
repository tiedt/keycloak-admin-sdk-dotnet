using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.Features;

internal abstract class RealmServiceBase(IKeycloakHttpClient httpClient, RealmName realm)
{
    protected IKeycloakHttpClient HttpClient { get; } = httpClient;

    protected string RealmEndpoint { get; } =
        $"admin/realms/{Uri.EscapeDataString(ValidateRealm(realm))}";

    protected static string RequireId(KeycloakResourceId id, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(id.Value))
        {
            throw new ArgumentException("A Keycloak resource identifier is required.", parameterName);
        }

        return Uri.EscapeDataString(id.Value);
    }

    protected static string RequireText(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value;
    }

    protected static KeycloakResourceId ReadLocationId(HttpResponseMessage response)
    {
        var location = response.Headers.Location?.Segments.LastOrDefault()?.Trim('/');
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new Exceptions.KeycloakApiException(
                "Keycloak created a resource without returning its identifier.",
                response.StatusCode);
        }

        return new KeycloakResourceId(Uri.UnescapeDataString(location));
    }

    private static string ValidateRealm(RealmName realm)
    {
        if (string.IsNullOrWhiteSpace(realm.Value))
        {
            throw new ArgumentException("A realm name is required.", nameof(realm));
        }

        return realm.Value;
    }
}
