using System.Net;
using Keycloak.AdminSdk.Abstractions;
using Keycloak.AdminSdk.Abstractions.Http;
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
using Keycloak.AdminSdk.Authentication;
using Keycloak.AdminSdk.Configuration;
using Keycloak.AdminSdk.Context;
using Keycloak.AdminSdk.Http;
using Keycloak.AdminSdk.Features.Realms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers the Keycloak Admin SDK in the native .NET dependency injection container.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the SDK using a delegate to configure its options.</summary>
    public static IServiceCollection AddKeycloakAdmin(
        this IServiceCollection services,
        Action<KeycloakAdminOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<KeycloakAdminOptions>()
            .Configure(configure)
            .ValidateOnStart();

        return AddKeycloakAdminCore(services);
    }

    /// <summary>Registers the SDK using an application configuration section.</summary>
    public static IServiceCollection AddKeycloakAdmin(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurationSection);

        services.AddOptions<KeycloakAdminOptions>()
            .Bind(configurationSection)
            .ValidateOnStart();

        return AddKeycloakAdminCore(services);
    }

    private static IServiceCollection AddKeycloakAdminCore(IServiceCollection services)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<KeycloakAdminOptions>, KeycloakAdminOptionsValidator>());
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IKeycloakTokenProvider, KeycloakTokenProvider>();
        services.TryAddTransient<AuthenticationDelegatingHandler>();
        services.TryAddSingleton<IKeycloakHttpClient, KeycloakHttpClient>();
        services.TryAddSingleton<IKeycloakRealmService, KeycloakRealmService>();
        services.TryAddSingleton<IKeycloakRealmContextFactory, KeycloakRealmContextFactory>();
        services.TryAddSingleton<IKeycloakAdminClient, KeycloakAdminClient>();
        services.TryAddTransient(static provider => GetDefaultRealm(provider).Users);
        services.TryAddTransient(static provider => GetDefaultRealm(provider).Clients);
        services.TryAddTransient(static provider => GetDefaultRealm(provider).ClientScopes);
        services.TryAddTransient(static provider => GetDefaultRealm(provider).Roles);
        services.TryAddTransient(static provider => GetDefaultRealm(provider).Groups);
        services.TryAddTransient(static provider => GetDefaultRealm(provider).ProtocolMappers);
        services.TryAddTransient(static provider => GetDefaultRealm(provider).IdentityProviders);
        services.TryAddTransient(static provider => GetDefaultRealm(provider).Sessions);
        services.TryAddTransient(static provider => GetDefaultRealm(provider).Events);

        AddTokenHttpClient(services);
        AddAdminHttpClient(services);

        return services;
    }

    private static IKeycloakRealmContext GetDefaultRealm(IServiceProvider provider) =>
        provider.GetRequiredService<IKeycloakAdminClient>().DefaultRealm
        ?? throw new InvalidOperationException(
            "DefaultRealm must be configured before resolving realm-bound services directly from DI.");

    private static void AddTokenHttpClient(IServiceCollection services)
    {
        var builder = services.AddHttpClient(
            KeycloakHttpClientNames.TokenEndpoint,
            ConfigureHttpClient);

        ConfigurePrimaryHandler(builder);
        builder.AddStandardResilienceHandler()
            .Configure(ConfigureTokenResilience);
    }

    private static void AddAdminHttpClient(IServiceCollection services)
    {
        var builder = services.AddHttpClient(
                KeycloakHttpClientNames.AdminApi,
                ConfigureHttpClient)
            .AddHttpMessageHandler<AuthenticationDelegatingHandler>();

        ConfigurePrimaryHandler(builder);
        builder.AddStandardResilienceHandler()
            .Configure(ConfigureAdminResilience);
    }

    private static void ConfigureHttpClient(IServiceProvider services, HttpClient client)
    {
        var options = services.GetRequiredService<IOptions<KeycloakAdminOptions>>().Value;
        client.BaseAddress = NormalizeBaseAddress(options.ServerUrl!);
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    }

    private static void ConfigurePrimaryHandler(IHttpClientBuilder builder)
    {
        builder.ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip |
                                     DecompressionMethods.Deflate |
                                     DecompressionMethods.Brotli,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            UseCookies = false,
        });
    }

    private static void ConfigureTokenResilience(
        HttpStandardResilienceOptions resilience,
        IServiceProvider services)
    {
        ApplyResilienceOptions(resilience, services);
    }

    private static void ConfigureAdminResilience(
        HttpStandardResilienceOptions resilience,
        IServiceProvider services)
    {
        ApplyResilienceOptions(resilience, services);
        resilience.Retry.DisableForUnsafeHttpMethods();
    }

    private static void ApplyResilienceOptions(
        HttpStandardResilienceOptions resilience,
        IServiceProvider services)
    {
        var settings = services.GetRequiredService<IOptions<KeycloakAdminOptions>>().Value.Resilience;
        resilience.Retry.MaxRetryAttempts = settings.MaxRetryAttempts;
        resilience.Retry.Delay = settings.RetryBaseDelay;
        resilience.Retry.BackoffType = DelayBackoffType.Exponential;
        resilience.Retry.UseJitter = true;
        resilience.AttemptTimeout.Timeout = settings.AttemptTimeout;
        resilience.TotalRequestTimeout.Timeout = settings.TotalRequestTimeout;
    }

    private static Uri NormalizeBaseAddress(Uri serverUrl)
    {
        var value = serverUrl.AbsoluteUri.EndsWith('/')
            ? serverUrl.AbsoluteUri
            : $"{serverUrl.AbsoluteUri}/";

        return new Uri(value, UriKind.Absolute);
    }
}
