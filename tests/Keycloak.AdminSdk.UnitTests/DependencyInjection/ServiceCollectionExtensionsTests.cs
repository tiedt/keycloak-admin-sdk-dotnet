using Keycloak.AdminSdk.Abstractions;
using Keycloak.AdminSdk.Abstractions.Realms;
using Keycloak.AdminSdk.Abstractions.Clients;
using Keycloak.AdminSdk.Abstractions.ClientScopes;
using Keycloak.AdminSdk.Abstractions.Groups;
using Keycloak.AdminSdk.Abstractions.Roles;
using Keycloak.AdminSdk.Abstractions.Users;
using Keycloak.AdminSdk.Abstractions.Events;
using Keycloak.AdminSdk.Abstractions.IdentityProviders;
using Keycloak.AdminSdk.Abstractions.ProtocolMappers;
using Keycloak.AdminSdk.Abstractions.Sessions;
using Keycloak.AdminSdk.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Keycloak.AdminSdk.UnitTests.DependencyInjection;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeycloakAdminRegistersRealmContextServicesAsSingletons()
    {
        var services = new ServiceCollection();
        services.AddKeycloakAdmin(options =>
        {
            options.ServerUrl = new Uri("https://identity.example/");
            options.DefaultRealm = "customers";
            options.Authentication.ClientCredentials.ClientId = "admin-client";
            options.Authentication.ClientCredentials.ClientSecret = "secret";
        });

        using var provider = services.BuildServiceProvider();
        var firstClient = provider.GetRequiredService<IKeycloakAdminClient>();
        var secondClient = provider.GetRequiredService<IKeycloakAdminClient>();
        var factory = provider.GetRequiredService<IKeycloakRealmContextFactory>();
        var realmService = provider.GetRequiredService<IKeycloakRealmService>();
        var defaultRealm = Assert.IsAssignableFrom<IKeycloakRealmContext>(firstClient.DefaultRealm);

        Assert.Same(firstClient, secondClient);
        Assert.Same(defaultRealm, factory.Create("customers"));
        Assert.Same(firstClient.Realms, realmService);
        Assert.Same(defaultRealm.Users, provider.GetRequiredService<IKeycloakUserService>());
        Assert.Same(defaultRealm.Clients, provider.GetRequiredService<IKeycloakClientService>());
        Assert.Same(defaultRealm.ClientScopes, provider.GetRequiredService<IKeycloakClientScopeService>());
        Assert.Same(defaultRealm.Roles, provider.GetRequiredService<IKeycloakRoleService>());
        Assert.Same(defaultRealm.Groups, provider.GetRequiredService<IKeycloakGroupService>());
        Assert.Same(defaultRealm.ProtocolMappers, provider.GetRequiredService<IKeycloakProtocolMapperService>());
        Assert.Same(defaultRealm.IdentityProviders, provider.GetRequiredService<IKeycloakIdentityProviderService>());
        Assert.Same(defaultRealm.Sessions, provider.GetRequiredService<IKeycloakSessionService>());
        Assert.Same(defaultRealm.Events, provider.GetRequiredService<IKeycloakEventService>());
    }
}
