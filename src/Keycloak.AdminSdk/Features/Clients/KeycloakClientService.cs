using System.Net.Http.Json;
using Keycloak.AdminSdk.Abstractions.Clients;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Abstractions.Roles;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Clients.Models;
using Keycloak.AdminSdk.Internal.Http;
using Keycloak.AdminSdk.Internal.Representations;
using Keycloak.AdminSdk.Internal.Serialization;

namespace Keycloak.AdminSdk.Features.Clients;

internal sealed class KeycloakClientService(
    IKeycloakHttpClient httpClient,
    RealmName realm,
    IKeycloakRoleService roleService)
    : RealmServiceBase(httpClient, realm), IKeycloakClientService
{
    private string ClientsEndpoint => $"{RealmEndpoint}/clients";

    public IClientProvisioningBuilder Define(CreateClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new ClientProvisioningBuilder(this, roleService, request);
    }

    public async Task<IReadOnlyList<ClientResource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ClientsEndpoint);
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var clients = await KeycloakResponse.ReadRequiredAsync<List<ClientRepresentation>>(
            response, "list clients", cancellationToken).ConfigureAwait(false);
        return clients.ConvertAll(ToResource);
    }

    public async Task<ClientResource> GetAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ClientEndpoint(clientId));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return ToResource(await KeycloakResponse.ReadRequiredAsync<ClientRepresentation>(
            response, "get client", cancellationToken).ConfigureAwait(false));
    }

    public async Task<ClientResource?> FindByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        RequireText(clientId, nameof(clientId));
        var endpoint = $"{ClientsEndpoint}?clientId={Uri.EscapeDataString(clientId)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var clients = await KeycloakResponse.ReadRequiredAsync<List<ClientRepresentation>>(
            response, "find client", cancellationToken).ConfigureAwait(false);
        return clients.Count == 0 ? null : ToResource(clients[0]);
    }

    public async Task<ClientResource> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireText(request.ClientId, nameof(request));
        using var message = new HttpRequestMessage(HttpMethod.Post, ClientsEndpoint)
        {
            Content = JsonContent.Create(ToRepresentation(request), options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "create client");
        return await GetAsync(ReadLocationId(response), cancellationToken).ConfigureAwait(false);
    }

    public async Task<ClientResource> UpdateAsync(KeycloakResourceId clientId, UpdateClientRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var current = await GetAsync(clientId, cancellationToken).ConfigureAwait(false);
        using var message = new HttpRequestMessage(HttpMethod.Put, ClientEndpoint(clientId))
        {
            Content = JsonContent.Create(ToRepresentation(current, request), options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "update client");
        return await GetAsync(clientId, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default) =>
        SendWithoutBodyAsync(HttpMethod.Delete, ClientEndpoint(clientId), "delete client", cancellationToken);

    public Task AddDefaultScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default) =>
        ChangeScopeAsync(HttpMethod.Put, clientId, scopeId, "default-client-scopes", "add default client scope", cancellationToken);

    public Task RemoveDefaultScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default) =>
        ChangeScopeAsync(HttpMethod.Delete, clientId, scopeId, "default-client-scopes", "remove default client scope", cancellationToken);

    public Task AddOptionalScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default) =>
        ChangeScopeAsync(HttpMethod.Put, clientId, scopeId, "optional-client-scopes", "add optional client scope", cancellationToken);

    public Task RemoveOptionalScopeAsync(KeycloakResourceId clientId, KeycloakResourceId scopeId, CancellationToken cancellationToken = default) =>
        ChangeScopeAsync(HttpMethod.Delete, clientId, scopeId, "optional-client-scopes", "remove optional client scope", cancellationToken);

    private Task ChangeScopeAsync(HttpMethod method, KeycloakResourceId clientId, KeycloakResourceId scopeId, string collection, string operation, CancellationToken token) =>
        SendWithoutBodyAsync(method, $"{ClientEndpoint(clientId)}/{collection}/{RequireId(scopeId, nameof(scopeId))}", operation, token);

    private async Task SendWithoutBodyAsync(HttpMethod method, string endpoint, string operation, CancellationToken token)
    {
        using var request = new HttpRequestMessage(method, endpoint);
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, operation);
    }

    private string ClientEndpoint(KeycloakResourceId id) => $"{ClientsEndpoint}/{RequireId(id, nameof(id))}";

    private static ClientResource ToResource(ClientRepresentation client)
    {
        if (string.IsNullOrWhiteSpace(client.Id) || string.IsNullOrWhiteSpace(client.ClientId))
            throw new Exceptions.KeycloakApiException("Keycloak returned an invalid client representation.");
        return new ClientResource
        {
            Id = new KeycloakResourceId(client.Id),
            ClientId = client.ClientId,
            Name = client.Name,
            Description = client.Description,
            Enabled = client.Enabled ?? false,
            PublicClient = client.PublicClient ?? false,
            ServiceAccountsEnabled = client.ServiceAccountsEnabled ?? false,
            StandardFlowEnabled = client.StandardFlowEnabled ?? false,
            DirectAccessGrantsEnabled = client.DirectAccessGrantsEnabled ?? false,
            Protocol = client.Protocol ?? "openid-connect",
            RedirectUris = client.RedirectUris ?? [],
            WebOrigins = client.WebOrigins ?? [],
        };
    }

    private static ClientRepresentation ToRepresentation(CreateClientRequest client) => new()
    {
        ClientId = client.ClientId,
        Name = client.Name,
        Description = client.Description,
        Secret = client.Secret,
        Enabled = client.Enabled,
        PublicClient = client.PublicClient,
        ServiceAccountsEnabled = client.ServiceAccountsEnabled,
        StandardFlowEnabled = client.StandardFlowEnabled,
        DirectAccessGrantsEnabled = client.DirectAccessGrantsEnabled,
        Protocol = client.Protocol,
        RedirectUris = client.RedirectUris.ToList(),
        WebOrigins = client.WebOrigins.ToList(),
    };

    private static ClientRepresentation ToRepresentation(ClientResource current, UpdateClientRequest client) => new()
    {
        Id = current.Id.Value,
        ClientId = client.ClientId ?? current.ClientId,
        Name = client.Name ?? current.Name,
        Description = client.Description ?? current.Description,
        Enabled = client.Enabled ?? current.Enabled,
        PublicClient = client.PublicClient ?? current.PublicClient,
        ServiceAccountsEnabled = client.ServiceAccountsEnabled ?? current.ServiceAccountsEnabled,
        StandardFlowEnabled = client.StandardFlowEnabled ?? current.StandardFlowEnabled,
        DirectAccessGrantsEnabled = client.DirectAccessGrantsEnabled ?? current.DirectAccessGrantsEnabled,
        Protocol = current.Protocol,
        RedirectUris = (client.RedirectUris ?? current.RedirectUris).ToList(),
        WebOrigins = (client.WebOrigins ?? current.WebOrigins).ToList(),
    };
}
