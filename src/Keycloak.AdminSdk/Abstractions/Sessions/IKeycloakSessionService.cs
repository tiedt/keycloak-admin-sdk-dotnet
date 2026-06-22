using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Sessions.Models;

namespace Keycloak.AdminSdk.Abstractions.Sessions;

/// <summary>Reads and revokes user sessions in one realm.</summary>
public interface IKeycloakSessionService
{
    Task<IReadOnlyList<UserSessionResource>> GetUserSessionsAsync(KeycloakResourceId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSessionResource>> GetClientSessionsAsync(KeycloakResourceId clientId, int first = 0, int max = 100, CancellationToken cancellationToken = default);
    Task RevokeAsync(string sessionId, CancellationToken cancellationToken = default);
    Task LogoutAllAsync(CancellationToken cancellationToken = default);
    Task PushClientRevocationAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default);
}
