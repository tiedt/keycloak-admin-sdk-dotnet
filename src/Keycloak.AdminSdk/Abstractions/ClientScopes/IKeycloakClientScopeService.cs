using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.ClientScopes.Models;

namespace Keycloak.AdminSdk.Abstractions.ClientScopes;

/// <summary>Manages client scopes in one realm.</summary>
public interface IKeycloakClientScopeService
{
    Task<IReadOnlyList<ClientScopeResource>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientScopeResource> GetAsync(KeycloakResourceId scopeId, CancellationToken cancellationToken = default);
    Task<ClientScopeResource?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<ClientScopeResource> CreateAsync(CreateClientScopeRequest request, CancellationToken cancellationToken = default);
    Task<ClientScopeResource> UpdateAsync(KeycloakResourceId scopeId, UpdateClientScopeRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(KeycloakResourceId scopeId, CancellationToken cancellationToken = default);
}
