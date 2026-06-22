using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Roles.Models;

namespace Keycloak.AdminSdk.Abstractions.Roles;

/// <summary>Manages realm roles, client roles, and role mappings.</summary>
public interface IKeycloakRoleService
{
    Task<IReadOnlyList<RoleResource>> GetRealmRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleResource?> FindRealmRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task<RoleResource> CreateRealmRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleResource> UpdateRealmRoleAsync(string roleName, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteRealmRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleResource>> GetClientRolesAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default);
    Task<RoleResource?> FindClientRoleAsync(KeycloakResourceId clientId, string roleName, CancellationToken cancellationToken = default);
    Task<RoleResource> CreateClientRoleAsync(KeycloakResourceId clientId, CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleResource> UpdateClientRoleAsync(KeycloakResourceId clientId, string roleName, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteClientRoleAsync(KeycloakResourceId clientId, string roleName, CancellationToken cancellationToken = default);
    Task AssignRealmRolesToUserAsync(KeycloakResourceId userId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default);
    Task RemoveRealmRolesFromUserAsync(KeycloakResourceId userId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default);
    Task AssignRealmRolesToGroupAsync(KeycloakResourceId groupId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default);
    Task RemoveRealmRolesFromGroupAsync(KeycloakResourceId groupId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default);
    Task AssignClientRolesToUserAsync(KeycloakResourceId userId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default);
    Task RemoveClientRolesFromUserAsync(KeycloakResourceId userId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default);
    Task AssignClientRolesToGroupAsync(KeycloakResourceId groupId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default);
    Task RemoveClientRolesFromGroupAsync(KeycloakResourceId groupId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default);
}
