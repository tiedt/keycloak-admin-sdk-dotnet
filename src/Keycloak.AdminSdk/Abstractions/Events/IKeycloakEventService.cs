using Keycloak.AdminSdk.Features.Events.Models;

namespace Keycloak.AdminSdk.Abstractions.Events;

/// <summary>Reads, clears, and configures realm events.</summary>
public interface IKeycloakEventService
{
    Task<IReadOnlyList<KeycloakEventResource>> GetEventsAsync(EventQuery? query = null, CancellationToken cancellationToken = default);
    Task ClearEventsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KeycloakAdminEventResource>> GetAdminEventsAsync(AdminEventQuery? query = null, CancellationToken cancellationToken = default);
    Task ClearAdminEventsAsync(CancellationToken cancellationToken = default);
    Task<EventsConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task UpdateConfigurationAsync(EventsConfiguration configuration, CancellationToken cancellationToken = default);
}
