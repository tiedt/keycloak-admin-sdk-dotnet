using System.Net;
using System.Net.Http.Json;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Abstractions.Realms;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Exceptions;
using Keycloak.AdminSdk.Features.Realms.Models;
using Keycloak.AdminSdk.Internal.Http;
using Keycloak.AdminSdk.Internal.Representations;
using Keycloak.AdminSdk.Internal.Serialization;

namespace Keycloak.AdminSdk.Features.Realms;

internal sealed class KeycloakRealmService(IKeycloakHttpClient httpClient) : IKeycloakRealmService
{
    private const string RealmsEndpoint = "admin/realms";

    public async Task<IReadOnlyList<RealmResource>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, RealmsEndpoint);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var representations = await KeycloakResponse
            .ReadRequiredAsync<List<RealmRepresentation>>(response, "list realms", cancellationToken)
            .ConfigureAwait(false);

        return representations.ConvertAll(RealmMapper.ToResource);
    }

    public async Task<RealmResource> GetAsync(
        RealmName realm,
        CancellationToken cancellationToken = default)
    {
        var resource = await FindAsync(realm, cancellationToken).ConfigureAwait(false);
        return resource ?? throw new KeycloakNotFoundException(
            $"Keycloak realm '{realm.Value}' was not found.");
    }

    public async Task<RealmResource?> FindAsync(
        RealmName realm,
        CancellationToken cancellationToken = default)
    {
        ValidateRealm(realm);

        using var request = new HttpRequestMessage(HttpMethod.Get, BuildRealmEndpoint(realm));
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        var representation = await KeycloakResponse
            .ReadRequiredAsync<RealmRepresentation>(response, "get realm", cancellationToken)
            .ConfigureAwait(false);
        return RealmMapper.ToResource(representation);
    }

    public async Task<RealmResource> CreateAsync(
        CreateRealmRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRealm(request.Name);

        using var message = new HttpRequestMessage(HttpMethod.Post, RealmsEndpoint)
        {
            Content = JsonContent.Create(
                RealmMapper.ToRepresentation(request),
                options: KeycloakJson.Options),
        };
        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "create realm");

        return await GetAsync(request.Name, cancellationToken).ConfigureAwait(false);
    }

    public async Task<RealmResource> UpdateAsync(
        RealmName realm,
        UpdateRealmRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRealm(realm);
        ArgumentNullException.ThrowIfNull(request);
        ValidateUpdate(request);

        using var message = new HttpRequestMessage(HttpMethod.Put, BuildRealmEndpoint(realm))
        {
            Content = JsonContent.Create(
                RealmMapper.ToRepresentation(request),
                options: KeycloakJson.Options),
        };
        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "update realm");

        return await GetAsync(realm, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(
        RealmName realm,
        CancellationToken cancellationToken = default)
    {
        ValidateRealm(realm);

        using var request = new HttpRequestMessage(HttpMethod.Delete, BuildRealmEndpoint(realm));
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "delete realm");
    }

    private static string BuildRealmEndpoint(RealmName realm) =>
        $"{RealmsEndpoint}/{Uri.EscapeDataString(realm.Value)}";

    private static void ValidateRealm(RealmName realm)
    {
        if (string.IsNullOrWhiteSpace(realm.Value))
        {
            throw new ArgumentException("A realm name is required.", nameof(realm));
        }
    }

    private static void ValidateUpdate(UpdateRealmRequest request)
    {
        var hasChanges = request.DisplayName is not null ||
                         request.Enabled is not null ||
                         request.RegistrationAllowed is not null ||
                         request.RegistrationEmailAsUsername is not null ||
                         request.RememberMe is not null ||
                         request.VerifyEmail is not null ||
                         request.LoginWithEmailAllowed is not null ||
                         request.DuplicateEmailsAllowed is not null ||
                         request.ResetPasswordAllowed is not null ||
                         request.EditUsernameAllowed is not null ||
                         request.BruteForceProtected is not null ||
                         request.SslRequired is not null;

        if (!hasChanges)
        {
            throw new ArgumentException("At least one realm property must be updated.", nameof(request));
        }
    }
}
