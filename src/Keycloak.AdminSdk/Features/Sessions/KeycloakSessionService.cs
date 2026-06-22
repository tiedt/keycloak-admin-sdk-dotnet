using System.Globalization;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Abstractions.Sessions;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Exceptions;
using Keycloak.AdminSdk.Features.Sessions.Models;
using Keycloak.AdminSdk.Internal.Http;
using Keycloak.AdminSdk.Internal.Representations;

namespace Keycloak.AdminSdk.Features.Sessions;

internal sealed class KeycloakSessionService(IKeycloakHttpClient httpClient, RealmName realm)
    : RealmServiceBase(httpClient, realm), IKeycloakSessionService
{
    public Task<IReadOnlyList<UserSessionResource>> GetUserSessionsAsync(KeycloakResourceId userId, CancellationToken cancellationToken = default) =>
        GetSessionsAsync($"{RealmEndpoint}/users/{RequireId(userId, nameof(userId))}/sessions", "list user sessions", cancellationToken);

    public Task<IReadOnlyList<UserSessionResource>> GetClientSessionsAsync(
        KeycloakResourceId clientId,
        int first = 0,
        int max = 100,
        CancellationToken cancellationToken = default)
    {
        if (first < 0 || max is < 1 or > 1_000) throw new ArgumentOutOfRangeException(nameof(max), "Session pagination is invalid.");
        var endpoint = $"{RealmEndpoint}/clients/{RequireId(clientId, nameof(clientId))}/user-sessions" +
                       $"?first={first.ToString(CultureInfo.InvariantCulture)}&max={max.ToString(CultureInfo.InvariantCulture)}";
        return GetSessionsAsync(endpoint, "list client sessions", cancellationToken);
    }

    public Task RevokeAsync(string sessionId, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, $"{RealmEndpoint}/sessions/{Uri.EscapeDataString(RequireText(sessionId, nameof(sessionId)))}", "revoke session", cancellationToken);

    public Task LogoutAllAsync(CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Post, $"{RealmEndpoint}/logout-all", "logout all sessions", cancellationToken);

    public Task PushClientRevocationAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Post, $"{RealmEndpoint}/clients/{RequireId(clientId, nameof(clientId))}/push-revocation", "push client revocation", cancellationToken);

    private async Task<IReadOnlyList<UserSessionResource>> GetSessionsAsync(string endpoint, string operation, CancellationToken token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        var values = await KeycloakResponse.ReadRequiredAsync<List<UserSessionRepresentation>>(
            response, operation, token).ConfigureAwait(false);
        return values.ConvertAll(ToResource);
    }

    private async Task SendAsync(HttpMethod method, string endpoint, string operation, CancellationToken token)
    {
        using var request = new HttpRequestMessage(method, endpoint);
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, operation);
    }

    private static UserSessionResource ToResource(UserSessionRepresentation value)
    {
        if (string.IsNullOrWhiteSpace(value.Id)) throw new KeycloakApiException("Keycloak returned an invalid session representation.");
        return new UserSessionResource
        {
            Id = value.Id,
            UserId = string.IsNullOrWhiteSpace(value.UserId) ? null : new KeycloakResourceId(value.UserId),
            Username = value.Username,
            IpAddress = value.IpAddress,
            StartedAtUtc = ToTimestamp(value.Start),
            LastAccessUtc = ToTimestamp(value.LastAccess),
            Clients = value.Clients ?? new Dictionary<string, string>(),
        };
    }

    private static DateTimeOffset? ToTimestamp(long? milliseconds) =>
        milliseconds is null ? null : DateTimeOffset.FromUnixTimeMilliseconds(milliseconds.Value);
}
