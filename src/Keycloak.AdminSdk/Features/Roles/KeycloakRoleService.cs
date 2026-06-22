using System.Net;
using System.Net.Http.Json;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Abstractions.Roles;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Roles.Models;
using Keycloak.AdminSdk.Internal.Http;
using Keycloak.AdminSdk.Internal.Representations;
using Keycloak.AdminSdk.Internal.Serialization;

namespace Keycloak.AdminSdk.Features.Roles;

internal sealed class KeycloakRoleService(IKeycloakHttpClient httpClient, RealmName realm)
    : RealmServiceBase(httpClient, realm), IKeycloakRoleService
{
    private string RolesEndpoint => $"{RealmEndpoint}/roles";

    public Task<IReadOnlyList<RoleResource>> GetRealmRolesAsync(CancellationToken cancellationToken = default) =>
        GetRolesAsync(RolesEndpoint, "list realm roles", cancellationToken);

    public async Task<RoleResource?> FindRealmRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        RequireText(roleName, nameof(roleName));
        using var request = new HttpRequestMessage(HttpMethod.Get, RoleEndpoint(roleName));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        return ToResource(await KeycloakResponse.ReadRequiredAsync<RoleRepresentation>(
            response, "get realm role", cancellationToken).ConfigureAwait(false));
    }

    public async Task<RoleResource> CreateRealmRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await CreateRoleAsync(RolesEndpoint, request, "create realm role", cancellationToken).ConfigureAwait(false);
        return await FindRealmRoleAsync(request.Name, cancellationToken).ConfigureAwait(false)
            ?? throw new Exceptions.KeycloakApiException("Keycloak created a realm role that could not be read.");
    }

    public async Task<RoleResource> UpdateRealmRoleAsync(string roleName, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        RequireText(roleName, nameof(roleName));
        ArgumentNullException.ThrowIfNull(request);
        var current = await FindRealmRoleAsync(roleName, cancellationToken).ConfigureAwait(false)
            ?? throw new Exceptions.KeycloakNotFoundException($"Keycloak realm role '{roleName}' was not found.");
        using var message = new HttpRequestMessage(HttpMethod.Put, RoleEndpoint(roleName))
        {
            Content = JsonContent.Create(new RoleRepresentation
            {
                Id = current.Id?.Value,
                Name = request.Name ?? current.Name,
                Description = request.Description ?? current.Description,
            }, options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "update realm role");
        return await FindRealmRoleAsync(request.Name ?? roleName, cancellationToken).ConfigureAwait(false)
            ?? throw new Exceptions.KeycloakApiException("Keycloak updated a realm role that could not be read.");
    }

    public async Task DeleteRealmRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        RequireText(roleName, nameof(roleName));
        using var request = new HttpRequestMessage(HttpMethod.Delete, RoleEndpoint(roleName));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "delete realm role");
    }

    public Task<IReadOnlyList<RoleResource>> GetClientRolesAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default) =>
        GetRolesAsync(ClientRolesEndpoint(clientId), "list client roles", cancellationToken);

    public Task<RoleResource?> FindClientRoleAsync(KeycloakResourceId clientId, string roleName, CancellationToken cancellationToken = default) =>
        FindRoleAsync($"{ClientRolesEndpoint(clientId)}/{Uri.EscapeDataString(RequireText(roleName, nameof(roleName)))}", "get client role", cancellationToken);

    public async Task<RoleResource> CreateClientRoleAsync(KeycloakResourceId clientId, CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var endpoint = ClientRolesEndpoint(clientId);
        await CreateRoleAsync(endpoint, request, "create client role", cancellationToken).ConfigureAwait(false);
        using var message = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/{Uri.EscapeDataString(request.Name)}");
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        return ToResource(await KeycloakResponse.ReadRequiredAsync<RoleRepresentation>(
            response, "get client role", cancellationToken).ConfigureAwait(false));
    }

    public async Task<RoleResource> UpdateClientRoleAsync(KeycloakResourceId clientId, string roleName, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var current = await FindClientRoleAsync(clientId, roleName, cancellationToken).ConfigureAwait(false)
            ?? throw new Exceptions.KeycloakNotFoundException($"Keycloak client role '{roleName}' was not found.");
        var endpoint = $"{ClientRolesEndpoint(clientId)}/{Uri.EscapeDataString(RequireText(roleName, nameof(roleName)))}";
        using var message = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = JsonContent.Create(new RoleRepresentation
            {
                Id = current.Id?.Value,
                Name = request.Name ?? current.Name,
                Description = request.Description ?? current.Description,
            }, options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "update client role");
        return await FindClientRoleAsync(clientId, request.Name ?? roleName, cancellationToken).ConfigureAwait(false)
            ?? throw new Exceptions.KeycloakApiException("Keycloak updated a client role that could not be read.");
    }

    public async Task DeleteClientRoleAsync(KeycloakResourceId clientId, string roleName, CancellationToken cancellationToken = default)
    {
        var endpoint = $"{ClientRolesEndpoint(clientId)}/{Uri.EscapeDataString(RequireText(roleName, nameof(roleName)))}";
        using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "delete client role");
    }

    public Task AssignRealmRolesToUserAsync(KeycloakResourceId userId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) =>
        ChangeMappingsAsync(HttpMethod.Post, $"{RealmEndpoint}/users/{RequireId(userId, nameof(userId))}/role-mappings/realm", roles, "assign realm roles to user", cancellationToken);

    public Task RemoveRealmRolesFromUserAsync(KeycloakResourceId userId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) =>
        ChangeMappingsAsync(HttpMethod.Delete, $"{RealmEndpoint}/users/{RequireId(userId, nameof(userId))}/role-mappings/realm", roles, "remove realm roles from user", cancellationToken);

    public Task AssignRealmRolesToGroupAsync(KeycloakResourceId groupId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) =>
        ChangeMappingsAsync(HttpMethod.Post, $"{RealmEndpoint}/groups/{RequireId(groupId, nameof(groupId))}/role-mappings/realm", roles, "assign realm roles to group", cancellationToken);

    public Task RemoveRealmRolesFromGroupAsync(KeycloakResourceId groupId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) =>
        ChangeMappingsAsync(HttpMethod.Delete, $"{RealmEndpoint}/groups/{RequireId(groupId, nameof(groupId))}/role-mappings/realm", roles, "remove realm roles from group", cancellationToken);

    public Task AssignClientRolesToUserAsync(KeycloakResourceId userId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) =>
        ChangeMappingsAsync(HttpMethod.Post, ClientMappingEndpoint("users", userId, clientId), roles, "assign client roles to user", cancellationToken);

    public Task RemoveClientRolesFromUserAsync(KeycloakResourceId userId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) =>
        ChangeMappingsAsync(HttpMethod.Delete, ClientMappingEndpoint("users", userId, clientId), roles, "remove client roles from user", cancellationToken);

    public Task AssignClientRolesToGroupAsync(KeycloakResourceId groupId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) =>
        ChangeMappingsAsync(HttpMethod.Post, ClientMappingEndpoint("groups", groupId, clientId), roles, "assign client roles to group", cancellationToken);

    public Task RemoveClientRolesFromGroupAsync(KeycloakResourceId groupId, KeycloakResourceId clientId, IReadOnlyCollection<RoleResource> roles, CancellationToken cancellationToken = default) =>
        ChangeMappingsAsync(HttpMethod.Delete, ClientMappingEndpoint("groups", groupId, clientId), roles, "remove client roles from group", cancellationToken);

    private async Task<IReadOnlyList<RoleResource>> GetRolesAsync(string endpoint, string operation, CancellationToken token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        var roles = await KeycloakResponse.ReadRequiredAsync<List<RoleRepresentation>>(response, operation, token).ConfigureAwait(false);
        return roles.ConvertAll(ToResource);
    }

    private async Task<RoleResource?> FindRoleAsync(string endpoint, string operation, CancellationToken token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        return ToResource(await KeycloakResponse.ReadRequiredAsync<RoleRepresentation>(response, operation, token).ConfigureAwait(false));
    }

    private async Task CreateRoleAsync(string endpoint, CreateRoleRequest request, string operation, CancellationToken token)
    {
        RequireText(request.Name, nameof(request));
        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(new RoleRepresentation { Name = request.Name, Description = request.Description }, options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, token).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, operation);
    }

    private async Task ChangeMappingsAsync(HttpMethod method, string endpoint, IReadOnlyCollection<RoleResource> roles, string operation, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(roles);
        if (roles.Count == 0) throw new ArgumentException("At least one role is required.", nameof(roles));
        var wireRoles = roles.Select(ToRepresentation).ToArray();
        using var message = new HttpRequestMessage(method, endpoint)
        {
            Content = JsonContent.Create(wireRoles, options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, token).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, operation);
    }

    private string RoleEndpoint(string name) => $"{RolesEndpoint}/{Uri.EscapeDataString(name)}";
    private string ClientRolesEndpoint(KeycloakResourceId id) => $"{RealmEndpoint}/clients/{RequireId(id, nameof(id))}/roles";
    private string ClientMappingEndpoint(string subjectType, KeycloakResourceId subjectId, KeycloakResourceId clientId) =>
        $"{RealmEndpoint}/{subjectType}/{RequireId(subjectId, nameof(subjectId))}/role-mappings/clients/{RequireId(clientId, nameof(clientId))}";

    private static RoleResource ToResource(RoleRepresentation role)
    {
        if (string.IsNullOrWhiteSpace(role.Name)) throw new Exceptions.KeycloakApiException("Keycloak returned an invalid role representation.");
        return new RoleResource
        {
            Id = string.IsNullOrWhiteSpace(role.Id) ? null : new KeycloakResourceId(role.Id),
            Name = role.Name,
            Description = role.Description,
            Composite = role.Composite ?? false,
            ClientRole = role.ClientRole ?? false,
            ContainerId = role.ContainerId,
        };
    }

    private static RoleRepresentation ToRepresentation(RoleResource role) => new()
    {
        Id = role.Id?.Value,
        Name = role.Name,
        Description = role.Description,
        Composite = role.Composite,
        ClientRole = role.ClientRole,
        ContainerId = role.ContainerId,
    };
}
