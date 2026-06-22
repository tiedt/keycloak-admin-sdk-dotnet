using System.Net.Http.Json;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Abstractions.ProtocolMappers;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Exceptions;
using Keycloak.AdminSdk.Features.ProtocolMappers.Models;
using Keycloak.AdminSdk.Internal.Http;
using Keycloak.AdminSdk.Internal.Representations;
using Keycloak.AdminSdk.Internal.Serialization;

namespace Keycloak.AdminSdk.Features.ProtocolMappers;

internal sealed class KeycloakProtocolMapperService(IKeycloakHttpClient httpClient, RealmName realm)
    : RealmServiceBase(httpClient, realm), IKeycloakProtocolMapperService
{
    public Task<IReadOnlyList<ProtocolMapperResource>> GetClientScopeMappersAsync(KeycloakResourceId scopeId, CancellationToken cancellationToken = default) =>
        GetAllAsync(ClientScopeEndpoint(scopeId), cancellationToken);

    public Task<ProtocolMapperResource> CreateClientScopeMapperAsync(KeycloakResourceId scopeId, CreateProtocolMapperRequest request, CancellationToken cancellationToken = default) =>
        CreateAsync(ClientScopeEndpoint(scopeId), request, cancellationToken);

    public Task<ProtocolMapperResource> UpdateClientScopeMapperAsync(KeycloakResourceId scopeId, KeycloakResourceId mapperId, UpdateProtocolMapperRequest request, CancellationToken cancellationToken = default) =>
        UpdateAsync(ClientScopeEndpoint(scopeId), mapperId, request, cancellationToken);

    public Task DeleteClientScopeMapperAsync(KeycloakResourceId scopeId, KeycloakResourceId mapperId, CancellationToken cancellationToken = default) =>
        DeleteAsync(ClientScopeEndpoint(scopeId), mapperId, cancellationToken);

    public Task<IReadOnlyList<ProtocolMapperResource>> GetClientMappersAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default) =>
        GetAllAsync(ClientEndpoint(clientId), cancellationToken);

    public Task<ProtocolMapperResource> CreateClientMapperAsync(KeycloakResourceId clientId, CreateProtocolMapperRequest request, CancellationToken cancellationToken = default) =>
        CreateAsync(ClientEndpoint(clientId), request, cancellationToken);

    public Task<ProtocolMapperResource> UpdateClientMapperAsync(KeycloakResourceId clientId, KeycloakResourceId mapperId, UpdateProtocolMapperRequest request, CancellationToken cancellationToken = default) =>
        UpdateAsync(ClientEndpoint(clientId), mapperId, request, cancellationToken);

    public Task DeleteClientMapperAsync(KeycloakResourceId clientId, KeycloakResourceId mapperId, CancellationToken cancellationToken = default) =>
        DeleteAsync(ClientEndpoint(clientId), mapperId, cancellationToken);

    private async Task<IReadOnlyList<ProtocolMapperResource>> GetAllAsync(string endpoint, CancellationToken token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        var values = await KeycloakResponse.ReadRequiredAsync<List<ProtocolMapperRepresentation>>(
            response, "list protocol mappers", token).ConfigureAwait(false);
        return values.ConvertAll(ToResource);
    }

    private async Task<ProtocolMapperResource> GetAsync(string endpoint, KeycloakResourceId mapperId, CancellationToken token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, MapperEndpoint(endpoint, mapperId));
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        return ToResource(await KeycloakResponse.ReadRequiredAsync<ProtocolMapperRepresentation>(
            response, "get protocol mapper", token).ConfigureAwait(false));
    }

    private async Task<ProtocolMapperResource> CreateAsync(string endpoint, CreateProtocolMapperRequest value, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(value);
        RequireText(value.Name, nameof(value));
        RequireText(value.ProtocolMapper, nameof(value));
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(ToRepresentation(value), options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "create protocol mapper");
        return await GetAsync(endpoint, ReadLocationId(response), token).ConfigureAwait(false);
    }

    private async Task<ProtocolMapperResource> UpdateAsync(string endpoint, KeycloakResourceId mapperId, UpdateProtocolMapperRequest value, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(value);
        var current = await GetAsync(endpoint, mapperId, token).ConfigureAwait(false);
        using var request = new HttpRequestMessage(HttpMethod.Put, MapperEndpoint(endpoint, mapperId))
        {
            Content = JsonContent.Create(ToRepresentation(current, value), options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "update protocol mapper");
        return await GetAsync(endpoint, mapperId, token).ConfigureAwait(false);
    }

    private async Task DeleteAsync(string endpoint, KeycloakResourceId mapperId, CancellationToken token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, MapperEndpoint(endpoint, mapperId));
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "delete protocol mapper");
    }

    private string ClientScopeEndpoint(KeycloakResourceId id) =>
        $"{RealmEndpoint}/client-scopes/{RequireId(id, nameof(id))}/protocol-mappers/models";
    private string ClientEndpoint(KeycloakResourceId id) =>
        $"{RealmEndpoint}/clients/{RequireId(id, nameof(id))}/protocol-mappers/models";
    private static string MapperEndpoint(string endpoint, KeycloakResourceId id) =>
        $"{endpoint}/{RequireId(id, nameof(id))}";

    private static ProtocolMapperResource ToResource(ProtocolMapperRepresentation value)
    {
        if (string.IsNullOrWhiteSpace(value.Id) || string.IsNullOrWhiteSpace(value.Name) ||
            string.IsNullOrWhiteSpace(value.Protocol) || string.IsNullOrWhiteSpace(value.ProtocolMapper))
            throw new KeycloakApiException("Keycloak returned an invalid protocol mapper representation.");
        return new ProtocolMapperResource
        {
            Id = new KeycloakResourceId(value.Id),
            Name = value.Name,
            Protocol = value.Protocol,
            ProtocolMapper = value.ProtocolMapper,
            ConsentRequired = value.ConsentRequired ?? false,
            ConsentText = value.ConsentText,
            Config = value.Config ?? new Dictionary<string, string>(),
        };
    }

    private static ProtocolMapperRepresentation ToRepresentation(CreateProtocolMapperRequest value) => new()
    {
        Name = value.Name,
        Protocol = value.Protocol,
        ProtocolMapper = value.ProtocolMapper,
        ConsentRequired = value.ConsentRequired,
        ConsentText = value.ConsentText,
        Config = value.Config.ToDictionary(),
    };

    private static ProtocolMapperRepresentation ToRepresentation(ProtocolMapperResource current, UpdateProtocolMapperRequest value) => new()
    {
        Id = current.Id.Value,
        Name = value.Name ?? current.Name,
        Protocol = value.Protocol ?? current.Protocol,
        ProtocolMapper = value.ProtocolMapper ?? current.ProtocolMapper,
        ConsentRequired = value.ConsentRequired ?? current.ConsentRequired,
        ConsentText = value.ConsentText ?? current.ConsentText,
        Config = (value.Config ?? current.Config).ToDictionary(),
    };
}
