using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Clients.Models;

namespace Keycloak.AdminSdk.Abstractions.Clients;

/// <summary>Manages clients in one realm.</summary>
public interface IKeycloakClientService
{
    IClientProvisioningBuilder Define(CreateClientRequest request);
    Task<IReadOnlyList<ClientResource>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientResource> GetAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default);
    Task<ClientResource?> FindByClientIdAsync(string clientId, CancellationToken cancellationToken = default);
    Task<ClientResource> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default);
    Task<ClientResource> UpdateAsync(KeycloakResourceId clientId, UpdateClientRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default);
    Task AddDefaultScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default);
    Task RemoveDefaultScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default);
    Task AddOptionalScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default);
    Task RemoveOptionalScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default);
}
