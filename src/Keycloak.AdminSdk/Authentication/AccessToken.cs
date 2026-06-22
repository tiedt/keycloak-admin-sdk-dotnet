namespace Keycloak.AdminSdk.Authentication;

internal sealed record AccessToken(string Value, DateTimeOffset RefreshAtUtc);
