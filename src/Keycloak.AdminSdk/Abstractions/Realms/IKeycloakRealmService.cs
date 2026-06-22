using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Realms.Models;

namespace Keycloak.AdminSdk.Abstractions.Realms;

/// <summary>Manages realms through the Keycloak Admin REST API.</summary>
public interface IKeycloakRealmService
{
    /// <summary>Gets all realms visible to the authenticated administrative client.</summary>
    Task<IReadOnlyList<RealmResource>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a realm or throws when it does not exist.</summary>
    Task<RealmResource> GetAsync(RealmName realm, CancellationToken cancellationToken = default);

    /// <summary>Gets a realm, returning <see langword="null"/> when it does not exist.</summary>
    Task<RealmResource?> FindAsync(RealmName realm, CancellationToken cancellationToken = default);

    /// <summary>Creates a realm and returns its persisted representation.</summary>
    Task<RealmResource> CreateAsync(
        CreateRealmRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a realm and returns its persisted representation.</summary>
    Task<RealmResource> UpdateAsync(
        RealmName realm,
        UpdateRealmRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a realm.</summary>
    Task DeleteAsync(RealmName realm, CancellationToken cancellationToken = default);
}
