using Keycloak.AdminSdk.Features.ClientScopes.Models;
using Keycloak.AdminSdk.Features.Clients.Models;
using Keycloak.AdminSdk.Features.Roles.Models;

namespace Keycloak.AdminSdk.Abstractions.Clients;

/// <summary>Builds and executes a client provisioning plan.</summary>
public interface IClientProvisioningBuilder
{
    IClientProvisioningBuilder WithDefaultScope(ClientScopeResource scope);
    IClientProvisioningBuilder WithOptionalScope(ClientScopeResource scope);
    IClientProvisioningBuilder WithClientRole(CreateRoleRequest role);
    IClientProvisioningBuilder KeepClientOnFailure();
    Task<ClientResource> ApplyAsync(CancellationToken cancellationToken = default);
}
