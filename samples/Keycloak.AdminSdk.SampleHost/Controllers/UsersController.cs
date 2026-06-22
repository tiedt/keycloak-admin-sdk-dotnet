using Keycloak.AdminSdk.Abstractions;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Users.Models;
using Keycloak.AdminSdk.SampleHost.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Keycloak.AdminSdk.SampleHost.Controllers;

[ApiController]
[Route("api/realms/{realmName}/users")]
[Authorize(Policy = SecurityPolicies.KeycloakAdministration)]
[EnableRateLimiting("keycloak-admin")]
public sealed class UsersController(IKeycloakAdminClient keycloak) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<UserResource>> GetAll(
        string realmName,
        [FromQuery] string? search,
        CancellationToken cancellationToken) =>
        keycloak.ForRealm(realmName).Users.GetAllAsync(
            new UserQuery { Search = search, Max = 100 },
            cancellationToken);

    [HttpPost]
    public async Task<ActionResult<UserResource>> Create(
        string realmName,
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await keycloak.ForRealm(realmName).Users.CreateAsync(request, cancellationToken);
        return Created($"api/realms/{realmName}/users/{user.Id.Value}", user);
    }

    [HttpPut("{userId}")]
    public Task<UserResource> Update(
        string realmName,
        string userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken) =>
        keycloak.ForRealm(realmName).Users.UpdateAsync(new KeycloakResourceId(userId), request, cancellationToken);

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(string realmName, string userId, CancellationToken cancellationToken)
    {
        await keycloak.ForRealm(realmName).Users.DeleteAsync(new KeycloakResourceId(userId), cancellationToken);
        return NoContent();
    }
}
