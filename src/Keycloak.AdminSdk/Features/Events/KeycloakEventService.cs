using System.Globalization;
using System.Net.Http.Json;
using Keycloak.AdminSdk.Abstractions.Events;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.Events.Models;
using Keycloak.AdminSdk.Internal.Http;
using Keycloak.AdminSdk.Internal.Representations;
using Keycloak.AdminSdk.Internal.Serialization;

namespace Keycloak.AdminSdk.Features.Events;

internal sealed class KeycloakEventService(IKeycloakHttpClient httpClient, RealmName realm)
    : RealmServiceBase(httpClient, realm), IKeycloakEventService
{
    private string EventsEndpoint => $"{RealmEndpoint}/events";
    private string AdminEventsEndpoint => $"{RealmEndpoint}/admin-events";
    private string ConfigEndpoint => $"{EventsEndpoint}/config";

    public async Task<IReadOnlyList<KeycloakEventResource>> GetEventsAsync(EventQuery? query = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildEventQuery(query));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var values = await KeycloakResponse.ReadRequiredAsync<List<EventRepresentation>>(
            response, "list events", cancellationToken).ConfigureAwait(false);
        return values.ConvertAll(ToResource);
    }

    public Task ClearEventsAsync(CancellationToken cancellationToken = default) =>
        DeleteAsync(EventsEndpoint, "clear events", cancellationToken);

    public async Task<IReadOnlyList<KeycloakAdminEventResource>> GetAdminEventsAsync(AdminEventQuery? query = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildAdminEventQuery(query));
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var values = await KeycloakResponse.ReadRequiredAsync<List<AdminEventRepresentation>>(
            response, "list admin events", cancellationToken).ConfigureAwait(false);
        return values.ConvertAll(ToResource);
    }

    public Task ClearAdminEventsAsync(CancellationToken cancellationToken = default) =>
        DeleteAsync(AdminEventsEndpoint, "clear admin events", cancellationToken);

    public async Task<EventsConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ConfigEndpoint);
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var value = await KeycloakResponse.ReadRequiredAsync<EventsConfigRepresentation>(
            response, "get event configuration", cancellationToken).ConfigureAwait(false);
        return new EventsConfiguration
        {
            EventsEnabled = value.EventsEnabled ?? false,
            AdminEventsEnabled = value.AdminEventsEnabled ?? false,
            AdminEventsDetailsEnabled = value.AdminEventsDetailsEnabled ?? false,
            EventsExpirationSeconds = value.EventsExpiration ?? 0,
            EnabledEventTypes = value.EnabledEventTypes ?? [],
            EventListeners = value.EventsListeners ?? [],
        };
    }

    public async Task UpdateConfigurationAsync(EventsConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        using var request = new HttpRequestMessage(HttpMethod.Put, ConfigEndpoint)
        {
            Content = JsonContent.Create(new EventsConfigRepresentation
            {
                EventsEnabled = configuration.EventsEnabled,
                AdminEventsEnabled = configuration.AdminEventsEnabled,
                AdminEventsDetailsEnabled = configuration.AdminEventsDetailsEnabled,
                EventsExpiration = configuration.EventsExpirationSeconds,
                EnabledEventTypes = configuration.EnabledEventTypes.ToList(),
                EventsListeners = configuration.EventListeners.ToList(),
            }, options: KeycloakJson.Options),
        };
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, "update event configuration");
    }

    private async Task DeleteAsync(string endpoint, string operation, CancellationToken token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        KeycloakResponse.EnsureSuccess(response, operation);
    }

    private string BuildEventQuery(EventQuery? query)
    {
        if (query is null) return EventsEndpoint;
        ValidatePagination(query.First, query.Max);
        var values = new List<string>();
        foreach (var type in query.Types) Add(values, "type", type);
        Add(values, "client", query.ClientId);
        Add(values, "user", query.UserId);
        Add(values, "dateFrom", query.DateFromUtc?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Add(values, "dateTo", query.DateToUtc?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        AddPagination(values, query.First, query.Max);
        return $"{EventsEndpoint}?{string.Join('&', values)}";
    }

    private string BuildAdminEventQuery(AdminEventQuery? query)
    {
        if (query is null) return AdminEventsEndpoint;
        ValidatePagination(query.First, query.Max);
        var values = new List<string>();
        foreach (var operation in query.OperationTypes) Add(values, "operationTypes", operation);
        foreach (var resource in query.ResourceTypes) Add(values, "resourceTypes", resource);
        Add(values, "resourcePath", query.ResourcePath);
        Add(values, "authClient", query.AuthClient);
        Add(values, "authUser", query.AuthUser);
        AddPagination(values, query.First, query.Max);
        return $"{AdminEventsEndpoint}?{string.Join('&', values)}";
    }

    private static void ValidatePagination(int first, int max)
    {
        if (first < 0 || max is < 1 or > 1_000) throw new ArgumentOutOfRangeException(nameof(max), "Event pagination is invalid.");
    }

    private static void AddPagination(List<string> values, int first, int max)
    {
        values.Add($"first={first.ToString(CultureInfo.InvariantCulture)}");
        values.Add($"max={max.ToString(CultureInfo.InvariantCulture)}");
    }

    private static void Add(List<string> values, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) values.Add($"{name}={Uri.EscapeDataString(value)}");
    }

    private static KeycloakEventResource ToResource(EventRepresentation value) => new()
    {
        TimestampUtc = ToTimestamp(value.Time),
        Type = value.Type,
        RealmId = value.RealmId,
        ClientId = value.ClientId,
        UserId = value.UserId,
        SessionId = value.SessionId,
        IpAddress = value.IpAddress,
        Error = value.Error,
        Details = value.Details ?? new Dictionary<string, string>(),
    };

    private static KeycloakAdminEventResource ToResource(AdminEventRepresentation value) => new()
    {
        TimestampUtc = ToTimestamp(value.Time),
        RealmId = value.RealmId,
        OperationType = value.OperationType,
        ResourceType = value.ResourceType,
        ResourcePath = value.ResourcePath,
        Error = value.Error,
    };

    private static DateTimeOffset? ToTimestamp(long? milliseconds) =>
        milliseconds is null ? null : DateTimeOffset.FromUnixTimeMilliseconds(milliseconds.Value);
}
