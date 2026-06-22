using Keycloak.AdminSdk.Abstractions;
using Keycloak.AdminSdk.Exceptions;
using Keycloak.AdminSdk.Features.Clients.Models;
using Keycloak.AdminSdk.Features.Roles.Models;
using Keycloak.AdminSdk.SampleHost.Contracts;
using Keycloak.AdminSdk.SampleHost.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Keycloak.AdminSdk.SampleHost.Controllers;

[ApiController]
[Route("api/realms/{realmName}/provisioning")]
[Authorize(Policy = SecurityPolicies.KeycloakAdministration)]
[EnableRateLimiting("keycloak-admin")]
public sealed class ProvisioningController(IKeycloakAdminClient keycloak) : ControllerBase
{
    [HttpPost("clients")]
    public async Task<ActionResult<ClientResource>> ProvisionClient(
        string realmName,
        ProvisionClientRequest request,
        CancellationToken cancellationToken)
    {
        var realm = keycloak.ForRealm(realmName);
        var builder = realm.Clients.Define(new CreateClientRequest(request.ClientId)
        {
            Name = request.DisplayName,
            Enabled = true,
        });

        foreach (var scopeName in request.DefaultScopeNames)
        {
            var scope = await realm.ClientScopes.FindByNameAsync(scopeName, cancellationToken)
                ?? throw new KeycloakNotFoundException($"Client scope '{scopeName}' was not found.");
            builder.WithDefaultScope(scope);
        }

        foreach (var roleName in request.ClientRoleNames)
        {
            builder.WithClientRole(new CreateRoleRequest(roleName));
        }

        var client = await builder.ApplyAsync(cancellationToken);
        return Created($"api/realms/{realmName}/clients/{client.Id.Value}", client);
    }
}
