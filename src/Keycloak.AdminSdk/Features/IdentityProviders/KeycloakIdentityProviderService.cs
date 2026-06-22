using System.Net;
using System.Net.Http.Json;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Abstractions.IdentityProviders;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Exceptions;
using Keycloak.AdminSdk.Features.IdentityProviders.Models;
using Keycloak.AdminSdk.Internal.Http;
using Keycloak.AdminSdk.Internal.Representations;
using Keycloak.AdminSdk.Internal.Serialization;

namespace Keycloak.AdminSdk.Features.IdentityProviders;

internal sealed class KeycloakIdentityProviderService(IKeycloakHttpClient httpClient, RealmName realm)
    : RealmServiceBase(httpClient, realm), IKeycloakIdentityProviderService
{
    private string Endpoint => $"{RealmEndpoint}/identity-provider/instances";

    public async Task<IReadOnlyList<IdentityProviderResource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var values = await KeycloakResponse.ReadRequiredAsync<List<IdentityProviderRepresentation>>(
            response, "list identity providers", cancellationToken).ConfigureAwait(false);
        return values.ConvertAll(ToResource);
    }

    public async Task<IdentityProviderResource?> FindAsync(string providerAlias, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, InstanceEndpoint(providerAlias));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        return ToResource(await KeycloakResponse.ReadRequiredAsync<IdentityProviderRepresentation>(
            response, "get identity provider", cancellationToken).ConfigureAwait(false));
    }

    public async Task<IdentityProviderResource> CreateAsync(CreateIdentityProviderRequest value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        RequireText(value.Alias, nameof(value));
        RequireText(value.ProviderId, nameof(value));
        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = JsonContent.Create(ToRepresentation(value), options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "create identity provider");
        return await FindAsync(value.Alias, cancellationToken).ConfigureAwait(false)
            ?? throw new KeycloakApiException("Keycloak created an identity provider that could not be read.");
    }

    public async Task<IdentityProviderResource> UpdateAsync(string providerAlias, UpdateIdentityProviderRequest value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        var current = await FindAsync(providerAlias, cancellationToken).ConfigureAwait(false)
            ?? throw new KeycloakNotFoundException($"Identity provider '{providerAlias}' was not found.");
        using var request = new HttpRequestMessage(HttpMethod.Put, InstanceEndpoint(providerAlias))
        {
            Content = JsonContent.Create(ToRepresentation(current, value), options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "update identity provider");
        return await FindAsync(providerAlias, cancellationToken).ConfigureAwait(false)
            ?? throw new KeycloakApiException("Keycloak updated an identity provider that could not be read.");
    }

    public async Task DeleteAsync(string providerAlias, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, InstanceEndpoint(providerAlias));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "delete identity provider");
    }

    private string InstanceEndpoint(string providerAlias) => $"{Endpoint}/{Uri.EscapeDataString(RequireText(providerAlias, nameof(providerAlias)))}";

    private static IdentityProviderResource ToResource(IdentityProviderRepresentation value)
    {
        if (string.IsNullOrWhiteSpace(value.Alias) || string.IsNullOrWhiteSpace(value.ProviderId))
            throw new KeycloakApiException("Keycloak returned an invalid identity provider representation.");
        return new IdentityProviderResource
        {
            Alias = value.Alias,
            ProviderId = value.ProviderId,
            DisplayName = value.DisplayName,
            Enabled = value.Enabled ?? false,
            TrustEmail = value.TrustEmail ?? false,
            StoreToken = value.StoreToken ?? false,
            LinkOnly = value.LinkOnly ?? false,
            Config = value.Config ?? new Dictionary<string, string>(),
        };
    }

    private static IdentityProviderRepresentation ToRepresentation(CreateIdentityProviderRequest value) => new()
    {
        Alias = value.Alias,
        ProviderId = value.ProviderId,
        DisplayName = value.DisplayName,
        Enabled = value.Enabled,
        TrustEmail = value.TrustEmail,
        StoreToken = value.StoreToken,
        LinkOnly = value.LinkOnly,
        Config = value.Config.ToDictionary(),
    };

    private static IdentityProviderRepresentation ToRepresentation(IdentityProviderResource current, UpdateIdentityProviderRequest value) => new()
    {
        Alias = current.Alias,
        ProviderId = current.ProviderId,
        DisplayName = value.DisplayName ?? current.DisplayName,
        Enabled = value.Enabled ?? current.Enabled,
        TrustEmail = value.TrustEmail ?? current.TrustEmail,
        StoreToken = value.StoreToken ?? current.StoreToken,
        LinkOnly = value.LinkOnly ?? current.LinkOnly,
        Config = (value.Config ?? current.Config).ToDictionary(),
    };
}
