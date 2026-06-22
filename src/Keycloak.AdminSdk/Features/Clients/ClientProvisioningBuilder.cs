using Keycloak.AdminSdk.Abstractions.Clients;
using Keycloak.AdminSdk.Abstractions.Roles;
using Keycloak.AdminSdk.Exceptions;
using Keycloak.AdminSdk.Features.ClientScopes.Models;
using Keycloak.AdminSdk.Features.Clients.Models;
using Keycloak.AdminSdk.Features.Roles.Models;

namespace Keycloak.AdminSdk.Features.Clients;

internal sealed class ClientProvisioningBuilder(
    IKeycloakClientService clients,
    IKeycloakRoleService roles,
    CreateClientRequest request) : IClientProvisioningBuilder
{
    private readonly List<ClientScopeResource> _defaultScopes = [];
    private readonly List<ClientScopeResource> _optionalScopes = [];
    private readonly List<CreateRoleRequest> _clientRoles = [];
    private bool _rollbackOnFailure = true;
    private bool _executed;

    public IClientProvisioningBuilder WithDefaultScope(ClientScopeResource scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        _defaultScopes.Add(scope);
        return this;
    }

    public IClientProvisioningBuilder WithOptionalScope(ClientScopeResource scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        _optionalScopes.Add(scope);
        return this;
    }

    public IClientProvisioningBuilder WithClientRole(CreateRoleRequest role)
    {
        ArgumentNullException.ThrowIfNull(role);
        _clientRoles.Add(role);
        return this;
    }

    public IClientProvisioningBuilder KeepClientOnFailure()
    {
        _rollbackOnFailure = false;
        return this;
    }

    public async Task<ClientResource> ApplyAsync(CancellationToken cancellationToken = default)
    {
        if (_executed)
        {
            throw new InvalidOperationException("A client provisioning plan can only be executed once.");
        }

        _executed = true;
        ClientResource? client = null;
        try
        {
            client = await clients.CreateAsync(request, cancellationToken).ConfigureAwait(false);
            foreach (var scope in _defaultScopes)
                await clients.AddDefaultScopeAsync(client.Id, scope.Id, cancellationToken).ConfigureAwait(false);
            foreach (var scope in _optionalScopes)
                await clients.AddOptionalScopeAsync(client.Id, scope.Id, cancellationToken).ConfigureAwait(false);
            foreach (var role in _clientRoles)
                await roles.CreateClientRoleAsync(client.Id, role, cancellationToken).ConfigureAwait(false);
            return client;
        }
        catch (Exception exception) when (exception is not KeycloakProvisioningException)
        {
            if (client is not null && _rollbackOnFailure)
            {
                try
                {
                    await clients.DeleteAsync(client.Id, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception rollbackException)
                {
                    throw new KeycloakProvisioningException(
                        "Client provisioning and its compensating deletion both failed.",
                        new AggregateException(exception, rollbackException));
                }
            }

            throw new KeycloakProvisioningException("Client provisioning failed.", exception);
        }
    }
}
