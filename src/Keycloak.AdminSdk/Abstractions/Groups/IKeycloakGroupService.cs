using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Groups.Models;

namespace Keycloak.AdminSdk.Abstractions.Groups;

/// <summary>Manages groups and memberships in one realm.</summary>
public interface IKeycloakGroupService
{
    Task<IReadOnlyList<GroupResource>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<GroupResource> GetAsync(KeycloakResourceId groupId, CancellationToken cancellationToken = default);
    Task<GroupResource> CreateAsync(CreateGroupRequest request, KeycloakResourceId? parentGroupId = null, CancellationToken cancellationToken = default);
    Task<GroupResource> UpdateAsync(KeycloakResourceId groupId, UpdateGroupRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(KeycloakResourceId groupId, CancellationToken cancellationToken = default);
    Task AddUserAsync(KeycloakResourceId groupId, KeycloakResourceId userId, CancellationToken cancellationToken = default);
    Task RemoveUserAsync(KeycloakResourceId groupId, KeycloakResourceId userId, CancellationToken cancellationToken = default);
}
