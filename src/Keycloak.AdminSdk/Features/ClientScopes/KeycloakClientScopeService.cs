using System.Net.Http.Json;
using Keycloak.AdminSdk.Abstractions.ClientScopes;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.ClientScopes.Models;
using Keycloak.AdminSdk.Internal.Http;
using Keycloak.AdminSdk.Internal.Representations;
using Keycloak.AdminSdk.Internal.Serialization;

namespace Keycloak.AdminSdk.Features.ClientScopes;

internal sealed class KeycloakClientScopeService(IKeycloakHttpClient httpClient, RealmName realm)
    : RealmServiceBase(httpClient, realm), IKeycloakClientScopeService
{
    private string ScopesEndpoint => $"{RealmEndpoint}/client-scopes";

    public async Task<IReadOnlyList<ClientScopeResource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ScopesEndpoint);
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var scopes = await KeycloakResponse.ReadRequiredAsync<List<ClientScopeRepresentation>>(
            response, "list client scopes", cancellationToken).ConfigureAwait(false);
        return scopes.ConvertAll(ToResource);
    }

    public async Task<ClientScopeResource> GetAsync(KeycloakResourceId scopeId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ScopeEndpoint(scopeId));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return ToResource(await KeycloakResponse.ReadRequiredAsync<ClientScopeRepresentation>(
            response, "get client scope", cancellationToken).ConfigureAwait(false));
    }

    public async Task<ClientScopeResource?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        RequireText(name, nameof(name));
        var scopes = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        return scopes.FirstOrDefault(scope => string.Equals(scope.Name, name, StringComparison.Ordinal));
    }

    public async Task<ClientScopeResource> CreateAsync(CreateClientScopeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireText(request.Name, nameof(request));
        using var message = new HttpRequestMessage(HttpMethod.Post, ScopesEndpoint)
        {
            Content = JsonContent.Create(ToRepresentation(request), options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "create client scope");
        return await GetAsync(ReadLocationId(response), cancellationToken).ConfigureAwait(false);
    }

    public async Task<ClientScopeResource> UpdateAsync(KeycloakResourceId scopeId, UpdateClientScopeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var current = await GetAsync(scopeId, cancellationToken).ConfigureAwait(false);
        using var message = new HttpRequestMessage(HttpMethod.Put, ScopeEndpoint(scopeId))
        {
            Content = JsonContent.Create(ToRepresentation(current, request), options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "update client scope");
        return await GetAsync(scopeId, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(KeycloakResourceId scopeId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, ScopeEndpoint(scopeId));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "delete client scope");
    }

    private string ScopeEndpoint(KeycloakResourceId id) => $"{ScopesEndpoint}/{RequireId(id, nameof(id))}";

    private static ClientScopeResource ToResource(ClientScopeRepresentation scope)
    {
        if (string.IsNullOrWhiteSpace(scope.Id) || string.IsNullOrWhiteSpace(scope.Name))
            throw new Exceptions.KeycloakApiException("Keycloak returned an invalid client scope representation.");
        return new ClientScopeResource
        {
            Id = new KeycloakResourceId(scope.Id),
            Name = scope.Name,
            Description = scope.Description,
            Protocol = scope.Protocol ?? "openid-connect",
            Attributes = scope.Attributes ?? new Dictionary<string, string>(),
        };
    }

    private static ClientScopeRepresentation ToRepresentation(CreateClientScopeRequest scope) => new()
    { Name = scope.Name, Description = scope.Description, Protocol = scope.Protocol, Attributes = scope.Attributes.ToDictionary() };

    private static ClientScopeRepresentation ToRepresentation(ClientScopeResource current, UpdateClientScopeRequest scope) => new()
    {
        Id = current.Id.Value,
        Name = scope.Name ?? current.Name,
        Description = scope.Description ?? current.Description,
        Protocol = scope.Protocol ?? current.Protocol,
        Attributes = (scope.Attributes ?? current.Attributes).ToDictionary(),
    };
}
