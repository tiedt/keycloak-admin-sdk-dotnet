using Keycloak.AdminSdk.Abstractions;
using Keycloak.AdminSdk.Configuration;
using Keycloak.AdminSdk.Features.ClientScopes.Models;
using Keycloak.AdminSdk.Features.Clients.Models;
using Keycloak.AdminSdk.Features.Groups.Models;
using Keycloak.AdminSdk.Features.Realms.Models;
using Keycloak.AdminSdk.Features.Roles.Models;
using Keycloak.AdminSdk.Features.Users.Models;
using Keycloak.AdminSdk.Features.Events.Models;
using Keycloak.AdminSdk.Features.IdentityProviders.Models;
using Keycloak.AdminSdk.Features.ProtocolMappers.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Keycloak.AdminSdk.IntegrationTests.Realms;

public sealed class RealmLifecycleTests
{
    [KeycloakIntegrationFact]
    public async Task RealmCanBeCreatedReadUpdatedListedAndDeleted()
    {
        var realmName = $"sdk-integration-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.AddKeycloakAdmin(options =>
        {
            options.ServerUrl = new Uri("http://localhost:8080");
            options.Authentication.Flow = KeycloakAuthenticationFlow.Password;
            options.Authentication.Password = new PasswordCredentialsOptions
            {
                ClientId = "admin-cli",
                Username = "admin",
                Password = "admin",
                AllowPasswordGrant = true,
            };
        });

        using var provider = services.BuildServiceProvider();
        var keycloak = provider.GetRequiredService<IKeycloakAdminClient>();

        try
        {
            var created = await keycloak.Realms.CreateAsync(new CreateRealmRequest(realmName)
            {
                DisplayName = "SDK Integration Realm",
                SslRequired = SslRequirement.None,
            });
            Assert.Equal(realmName, created.Name.Value);

            var updated = await keycloak.Realms.UpdateAsync(
                realmName,
                new UpdateRealmRequest
                {
                    DisplayName = "Updated SDK Integration Realm",
                    RegistrationAllowed = true,
                });
            Assert.Equal("Updated SDK Integration Realm", updated.DisplayName);
            Assert.True(updated.RegistrationAllowed);

            var realms = await keycloak.Realms.GetAllAsync();
            Assert.Contains(realms, realm => realm.Name.Value == realmName);

            var context = keycloak.ForRealm(realmName);
            var scope = await context.ClientScopes.CreateAsync(
                new CreateClientScopeRequest("sdk-scope") { Description = "Integration scope" });

            var client = await context.Clients
                .Define(new CreateClientRequest("sdk-client")
                {
                    Name = "SDK Client",
                    PublicClient = true,
                })
                .WithDefaultScope(scope)
                .WithClientRole(new CreateRoleRequest("client-reader"))
                .ApplyAsync();
            var foundClient = await context.Clients.FindByClientIdAsync("sdk-client");
            Assert.NotNull(foundClient);
            Assert.Equal(client.Id, foundClient.Id);
            Assert.Equal("sdk-client", foundClient.ClientId);

            var user = await context.Users.CreateAsync(new CreateUserRequest("sdk-user")
            {
                Email = "sdk-user@example.test",
                FirstName = "SDK",
                LastName = "User",
            });
            await context.Users.ResetPasswordAsync(
                user.Id,
                new SetPasswordRequest("Temporary-Password-123!") { Temporary = true });
            user = await context.Users.UpdateAsync(user.Id, new UpdateUserRequest { EmailVerified = true });
            Assert.True(user.EmailVerified);

            var group = await context.Groups.CreateAsync(new CreateGroupRequest("sdk-group"));
            await context.Groups.AddUserAsync(group.Id, user.Id);
            group = await context.Groups.UpdateAsync(group.Id, new UpdateGroupRequest("sdk-group-updated"));
            Assert.Equal("sdk-group-updated", group.Name);

            var role = await context.Roles.CreateRealmRoleAsync(new CreateRoleRequest("sdk-operator"));
            await context.Roles.AssignRealmRolesToUserAsync(user.Id, [role]);
            await context.Roles.AssignRealmRolesToGroupAsync(group.Id, [role]);
            await context.Roles.RemoveRealmRolesFromUserAsync(user.Id, [role]);
            await context.Roles.RemoveRealmRolesFromGroupAsync(group.Id, [role]);

            var clientRole = Assert.Single(await context.Roles.GetClientRolesAsync(client.Id));
            await context.Roles.AssignClientRolesToUserAsync(user.Id, client.Id, [clientRole]);
            await context.Roles.AssignClientRolesToGroupAsync(group.Id, client.Id, [clientRole]);
            await context.Roles.RemoveClientRolesFromUserAsync(user.Id, client.Id, [clientRole]);
            await context.Roles.RemoveClientRolesFromGroupAsync(group.Id, client.Id, [clientRole]);
            clientRole = await context.Roles.UpdateClientRoleAsync(
                client.Id,
                clientRole.Name,
                new UpdateRoleRequest { Description = "Updated client role" });
            Assert.Equal("Updated client role", clientRole.Description);

            scope = await context.ClientScopes.UpdateAsync(
                scope.Id,
                new UpdateClientScopeRequest { Description = "Updated integration scope" });
            Assert.Equal("Updated integration scope", scope.Description);

            client = await context.Clients.UpdateAsync(
                client.Id,
                new UpdateClientRequest { Description = "Updated integration client" });
            Assert.Equal("Updated integration client", client.Description);

            var mapper = await context.ProtocolMappers.CreateClientScopeMapperAsync(
                scope.Id,
                new CreateProtocolMapperRequest("department-claim", "oidc-usermodel-attribute-mapper")
                {
                    Config = new Dictionary<string, string>
                    {
                        ["user.attribute"] = "department",
                        ["claim.name"] = "department",
                        ["jsonType.label"] = "String",
                        ["id.token.claim"] = "true",
                        ["access.token.claim"] = "true",
                    },
                });
            Assert.Contains(
                await context.ProtocolMappers.GetClientScopeMappersAsync(scope.Id),
                candidate => candidate.Id == mapper.Id);

            var identityProvider = await context.IdentityProviders.CreateAsync(
                new CreateIdentityProviderRequest("sdk-oidc", "oidc")
                {
                    DisplayName = "SDK OIDC",
                    Enabled = false,
                    Config = new Dictionary<string, string>
                    {
                        ["authorizationUrl"] = "https://idp.example.test/authorize",
                        ["tokenUrl"] = "https://idp.example.test/token",
                        ["clientId"] = "sdk-client",
                        ["clientSecret"] = "integration-only",
                        ["useJwksUrl"] = "false",
                        ["validateSignature"] = "false",
                    },
                });
            Assert.Equal("sdk-oidc", identityProvider.Alias);

            Assert.Empty(await context.Sessions.GetUserSessionsAsync(user.Id));
            Assert.Empty(await context.Sessions.GetClientSessionsAsync(client.Id));

            var eventConfiguration = await context.Events.GetConfigurationAsync();
            await context.Events.UpdateConfigurationAsync(eventConfiguration with
            {
                EventsEnabled = true,
                AdminEventsEnabled = true,
                AdminEventsDetailsEnabled = true,
            });
            Assert.NotNull(await context.Events.GetEventsAsync(new EventQuery { Max = 10 }));
            Assert.NotNull(await context.Events.GetAdminEventsAsync(new AdminEventQuery { Max = 10 }));
        }
        finally
        {
            if (await keycloak.Realms.FindAsync(realmName) is not null)
            {
                await keycloak.Realms.DeleteAsync(realmName);
            }
        }

        Assert.Null(await keycloak.Realms.FindAsync(realmName));
    }
}
