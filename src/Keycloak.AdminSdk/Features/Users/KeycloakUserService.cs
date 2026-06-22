using System.Globalization;
using System.Net.Http.Json;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Abstractions.Users;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Users.Models;
using Keycloak.AdminSdk.Internal.Http;
using Keycloak.AdminSdk.Internal.Representations;
using Keycloak.AdminSdk.Internal.Serialization;

namespace Keycloak.AdminSdk.Features.Users;

internal sealed class KeycloakUserService(IKeycloakHttpClient httpClient, RealmName realm)
    : RealmServiceBase(httpClient, realm), IKeycloakUserService
{
    private string UsersEndpoint => $"{RealmEndpoint}/users";

    public async Task<IReadOnlyList<UserResource>> GetAllAsync(
        UserQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildQuery(query));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var users = await KeycloakResponse.ReadRequiredAsync<List<UserRepresentation>>(
            response, "list users", cancellationToken).ConfigureAwait(false);
        return users.ConvertAll(ToResource);
    }

    public async Task<UserResource> GetAsync(KeycloakResourceId userId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, UserEndpoint(userId));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var user = await KeycloakResponse.ReadRequiredAsync<UserRepresentation>(
            response, "get user", cancellationToken).ConfigureAwait(false);
        return ToResource(user);
    }

    public async Task<UserResource?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        RequireText(username, nameof(username));
        var users = await GetAllAsync(new UserQuery { Username = username, Exact = true, Max = 2 }, cancellationToken)
            .ConfigureAwait(false);
        return users.Count == 0 ? null : users[0];
    }

    public async Task<UserResource> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireText(request.Username, nameof(request));
        using var message = new HttpRequestMessage(HttpMethod.Post, UsersEndpoint)
        {
            Content = JsonContent.Create(ToRepresentation(request), options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "create user");
        return await GetAsync(ReadLocationId(response), cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserResource> UpdateAsync(
        KeycloakResourceId userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var message = new HttpRequestMessage(HttpMethod.Put, UserEndpoint(userId))
        {
            Content = JsonContent.Create(ToRepresentation(request), options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "update user");
        return await GetAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(KeycloakResourceId userId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, UserEndpoint(userId));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "delete user");
    }

    public async Task ResetPasswordAsync(
        KeycloakResourceId userId,
        SetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireText(request.Password, nameof(request));
        using var message = new HttpRequestMessage(HttpMethod.Put, $"{UserEndpoint(userId)}/reset-password")
        {
            Content = JsonContent.Create(new CredentialRepresentation
            {
                Value = request.Password,
                Temporary = request.Temporary,
            }, options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "reset user password");
    }

    private string UserEndpoint(KeycloakResourceId userId) =>
        $"{UsersEndpoint}/{RequireId(userId, nameof(userId))}";

    private string BuildQuery(UserQuery? query)
    {
        if (query is null)
        {
            return UsersEndpoint;
        }

        if (query.First < 0 || query.Max < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(query), "Pagination values are invalid.");
        }

        var values = new List<string>();
        Add(values, "search", query.Search);
        Add(values, "username", query.Username);
        Add(values, "email", query.Email);
        if (query.Exact) values.Add("exact=true");
        if (query.First is not null) values.Add($"first={query.First.Value.ToString(CultureInfo.InvariantCulture)}");
        if (query.Max is not null) values.Add($"max={query.Max.Value.ToString(CultureInfo.InvariantCulture)}");
        return values.Count == 0 ? UsersEndpoint : $"{UsersEndpoint}?{string.Join('&', values)}";
    }

    private static void Add(List<string> values, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            values.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }

    private static UserResource ToResource(UserRepresentation user)
    {
        if (string.IsNullOrWhiteSpace(user.Id) || string.IsNullOrWhiteSpace(user.Username))
        {
            throw new Exceptions.KeycloakApiException("Keycloak returned an invalid user representation.");
        }

        return new UserResource
        {
            Id = new KeycloakResourceId(user.Id),
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Enabled = user.Enabled ?? false,
            EmailVerified = user.EmailVerified ?? false,
            Attributes = ToPublicAttributes(user.Attributes),
        };
    }

    private static UserRepresentation ToRepresentation(CreateUserRequest user) => new()
    {
        Username = user.Username,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Enabled = user.Enabled,
        EmailVerified = user.EmailVerified,
        Attributes = ToWireAttributes(user.Attributes),
    };

    private static UserRepresentation ToRepresentation(UpdateUserRequest user) => new()
    {
        Username = user.Username,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Enabled = user.Enabled,
        EmailVerified = user.EmailVerified,
        Attributes = user.Attributes is null ? null : ToWireAttributes(user.Attributes),
    };

    private static Dictionary<string, List<string>> ToWireAttributes(
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> attributes) =>
        attributes.ToDictionary(pair => pair.Key, pair => pair.Value.ToList(), StringComparer.Ordinal);

    private static Dictionary<string, IReadOnlyCollection<string>> ToPublicAttributes(
        Dictionary<string, List<string>>? attributes) =>
        attributes?.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyCollection<string>)pair.Value,
            StringComparer.Ordinal) ?? new Dictionary<string, IReadOnlyCollection<string>>();
}
