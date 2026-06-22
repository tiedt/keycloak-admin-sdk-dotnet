using Keycloak.AdminSdk.Abstractions;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Realms.Models;
using Keycloak.AdminSdk.SampleHost.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Keycloak.AdminSdk.SampleHost.Controllers;

[ApiController]
[Route("api/realms")]
[Authorize(Policy = SecurityPolicies.KeycloakAdministration)]
[EnableRateLimiting("keycloak-admin")]
public sealed class RealmsController(IKeycloakAdminClient keycloak) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<RealmResource>> GetAll(CancellationToken cancellationToken) =>
        keycloak.Realms.GetAllAsync(cancellationToken);

    [HttpGet("{realmName}")]
    public Task<RealmResource> Get(string realmName, CancellationToken cancellationToken) =>
        keycloak.Realms.GetAsync(new RealmName(realmName), cancellationToken);

    [HttpPost]
    public async Task<ActionResult<RealmResource>> Create(CreateRealmRequest request, CancellationToken cancellationToken)
    {
        var realm = await keycloak.Realms.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { realmName = realm.Name.Value }, realm);
    }

    [HttpPut("{realmName}")]
    public Task<RealmResource> Update(string realmName, UpdateRealmRequest request, CancellationToken cancellationToken) =>
        keycloak.Realms.UpdateAsync(new RealmName(realmName), request, cancellationToken);

    [HttpDelete("{realmName}")]
    public async Task<IActionResult> Delete(string realmName, CancellationToken cancellationToken)
    {
        await keycloak.Realms.DeleteAsync(new RealmName(realmName), cancellationToken);
        return NoContent();
    }
}
