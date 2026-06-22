using Keycloak.AdminSdk.Features.IdentityProviders.Models;

namespace Keycloak.AdminSdk.Abstractions.IdentityProviders;

/// <summary>Manages identity provider instances in one realm.</summary>
public interface IKeycloakIdentityProviderService
{
    Task<IReadOnlyList<IdentityProviderResource>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IdentityProviderResource?> FindAsync(string providerAlias, CancellationToken cancellationToken = default);
    Task<IdentityProviderResource> CreateAsync(CreateIdentityProviderRequest request, CancellationToken cancellationToken = default);
    Task<IdentityProviderResource> UpdateAsync(string providerAlias, UpdateIdentityProviderRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string providerAlias, CancellationToken cancellationToken = default);
}
