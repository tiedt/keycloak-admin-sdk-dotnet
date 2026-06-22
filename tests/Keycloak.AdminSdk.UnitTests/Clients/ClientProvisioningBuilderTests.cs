using Keycloak.AdminSdk.Abstractions.Clients;
using Keycloak.AdminSdk.Abstractions.Roles;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Exceptions;
using Keycloak.AdminSdk.Features.ClientScopes.Models;
using Keycloak.AdminSdk.Features.Clients;
using Keycloak.AdminSdk.Features.Clients.Models;
using Keycloak.AdminSdk.Features.Roles.Models;

namespace Keycloak.AdminSdk.UnitTests.Clients;

public sealed class ClientProvisioningBuilderTests
{
    [Fact]
    public async Task FailedAssociationDeletesNewClientByDefault()
    {
        var clients = new StubClientService { FailScopeAssociation = true };
        var builder = new ClientProvisioningBuilder(
                clients,
                new StubRoleService(),
                new CreateClientRequest("api"))
            .WithDefaultScope(new ClientScopeResource
            {
                Id = new KeycloakResourceId("scope-id"),
                Name = "scope",
            });

        await Assert.ThrowsAsync<KeycloakProvisioningException>(() => builder.ApplyAsync());

        Assert.True(clients.WasDeleted);
    }

    [Fact]
    public async Task PlanCannotExecuteMoreThanOnce()
    {
        var builder = new ClientProvisioningBuilder(
            new StubClientService(),
            new StubRoleService(),
            new CreateClientRequest("api"));

        await builder.ApplyAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => builder.ApplyAsync());
    }

    private sealed class StubClientService : IKeycloakClientService
    {
        private static readonly ClientResource Client = new()
        {
            Id = new KeycloakResourceId("client-id"),
            ClientId = "api",
        };

        public bool FailScopeAssociation { get; init; }
        public bool WasDeleted { get; private set; }
        public IClientProvisioningBuilder Define(CreateClientRequest request) => throw new NotSupportedException();
        public Task<ClientResource> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default) => Task.FromResult(Client);
        public Task AddDefaultScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default) =>
            FailScopeAssociation ? Task.FromException(new HttpRequestException("failure")) : Task.CompletedTask;
        public Task DeleteAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default) { WasDeleted = true; return Task.CompletedTask; }
        public Task AddOptionalScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveDefaultScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveOptionalScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<ClientResource>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ClientResource> GetAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ClientResource?> FindByClientIdAsync(string clientId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ClientResource> UpdateAsync(KeycloakResourceId clientId, UpdateClientRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class StubRoleService : IKeycloakRoleService
    {
        public Task<RoleResource> CreateClientRoleAsync(KeycloakResourceId clientId, CreateRoleRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new RoleResource { Name = request.Name });
        public Task<RoleResource?> FindClientRoleAsync(KeycloakResourceId clientId, string roleName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RoleResource> UpdateClientRoleAsync(KeycloakResourceId clientId, string roleName, UpdateRoleRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task DeleteClientRoleAsync(KeycloakResourceId clientId, string roleName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<RoleResource>> GetRealmRolesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RoleResource?> FindRealmRoleAsync(string roleName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RoleResource> CreateRealmRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RoleResource> UpdateRealmRoleAsync(string roleName, UpdateRoleRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task DeleteRealmRoleAsync(string roleName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<RoleResource>> GetClientRolesAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AssignRealmRolesToUserAsync(KeycloakResourceId userId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task RemoveRealmRolesFromUserAsync(KeycloakResourceId userId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AssignRealmRolesToGroupAsync(KeycloakResourceId groupId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task RemoveRealmRolesFromGroupAsync(KeycloakResourceId groupId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AssignClientRolesToUserAsync(KeycloakResourceId userId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task RemoveClientRolesFromUserAsync(KeycloakResourceId userId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AssignClientRolesToGroupAsync(KeycloakResourceId groupId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task RemoveClientRolesFromGroupAsync(KeycloakResourceId groupId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
