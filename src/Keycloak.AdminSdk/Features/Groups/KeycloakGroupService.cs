using System.Net.Http.Json;
using Keycloak.AdminSdk.Abstractions.Groups;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Groups.Models;
using Keycloak.AdminSdk.Internal.Http;
using Keycloak.AdminSdk.Internal.Representations;
using Keycloak.AdminSdk.Internal.Serialization;

namespace Keycloak.AdminSdk.Features.Groups;

internal sealed class KeycloakGroupService(IKeycloakHttpClient httpClient, RealmName realm)
    : RealmServiceBase(httpClient, realm), IKeycloakGroupService
{
    private string GroupsEndpoint => $"{RealmEndpoint}/groups";

    public async Task<IReadOnlyList<GroupResource>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var endpoint = string.IsNullOrWhiteSpace(search) ? GroupsEndpoint : $"{GroupsEndpoint}?search={Uri.EscapeDataString(search)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var groups = await KeycloakResponse.ReadRequiredAsync<List<GroupRepresentation>>(
            response, "list groups", cancellationToken).ConfigureAwait(false);
        return groups.ConvertAll(ToResource);
    }

    public async Task<GroupResource> GetAsync(KeycloakResourceId groupId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, GroupEndpoint(groupId));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return ToResource(await KeycloakResponse.ReadRequiredAsync<GroupRepresentation>(
            response, "get group", cancellationToken).ConfigureAwait(false));
    }

    public async Task<GroupResource> CreateAsync(CreateGroupRequest request, KeycloakResourceId? parentGroupId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireText(request.Name, nameof(request));
        var endpoint = parentGroupId is null ? GroupsEndpoint : $"{GroupEndpoint(parentGroupId.Value)}/children";
        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(new GroupRepresentation { Name = request.Name }, options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "create group");
        return await GetAsync(ReadLocationId(response), cancellationToken).ConfigureAwait(false);
    }

    public async Task<GroupResource> UpdateAsync(KeycloakResourceId groupId, UpdateGroupRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireText(request.Name, nameof(request));
        using var message = new HttpRequestMessage(HttpMethod.Put, GroupEndpoint(groupId))
        {
            Content = JsonContent.Create(new GroupRepresentation { Name = request.Name }, options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "update group");
        return await GetAsync(groupId, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(KeycloakResourceId groupId, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, GroupEndpoint(groupId), "delete group", cancellationToken);

    public Task AddUserAsync(KeycloakResourceId groupId, KeycloakResourceId userId, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Put, MembershipEndpoint(groupId, userId), "add user to group", cancellationToken);

    public Task RemoveUserAsync(KeycloakResourceId groupId, KeycloakResourceId userId, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, MembershipEndpoint(groupId, userId), "remove user from group", cancellationToken);

    private async Task SendAsync(HttpMethod method, string endpoint, string operation, CancellationToken token)
    {
        using var request = new HttpRequestMessage(method, endpoint);
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, operation);
    }

    private string GroupEndpoint(KeycloakResourceId id) => $"{GroupsEndpoint}/{RequireId(id, nameof(id))}";
    private string MembershipEndpoint(KeycloakResourceId groupId, KeycloakResourceId userId) =>
        $"{RealmEndpoint}/users/{RequireId(userId, nameof(userId))}/groups/{RequireId(groupId, nameof(groupId))}";

    private static GroupResource ToResource(GroupRepresentation group)
    {
        if (string.IsNullOrWhiteSpace(group.Id) || string.IsNullOrWhiteSpace(group.Name))
            throw new Exceptions.KeycloakApiException("Keycloak returned an invalid group representation.");
        return new GroupResource
        {
            Id = new KeycloakResourceId(group.Id),
            Name = group.Name,
            Path = group.Path,
            SubGroups = group.SubGroups?.ConvertAll(ToResource) ?? [],
        };
    }
}
