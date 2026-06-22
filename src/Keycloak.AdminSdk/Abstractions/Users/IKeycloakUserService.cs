using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Users.Models;

namespace Keycloak.AdminSdk.Abstractions.Users;

/// <summary>Manages users in one realm.</summary>
public interface IKeycloakUserService
{
    Task<IReadOnlyList<UserResource>> GetAllAsync(UserQuery? query = null, CancellationToken cancellationToken = default);
    Task<UserResource> GetAsync(KeycloakResourceId userId, CancellationToken cancellationToken = default);
    Task<UserResource?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserResource> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResource> UpdateAsync(KeycloakResourceId userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(KeycloakResourceId userId, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(KeycloakResourceId userId, SetPasswordRequest request, CancellationToken cancellationToken = default);
}
