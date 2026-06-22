using System.Diagnostics;
using System.Net.Http.Json;
using Keycloak.AdminSdk.Configuration;
using Keycloak.AdminSdk.Exceptions;
using Keycloak.AdminSdk.Http;
using Keycloak.AdminSdk.Observability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Keycloak.AdminSdk.Authentication;

internal sealed class KeycloakTokenProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<KeycloakAdminOptions> options,
    TimeProvider timeProvider,
    ILogger<KeycloakTokenProvider> logger) : IKeycloakTokenProvider, IDisposable
{
    private static readonly Action<ILogger, int, Exception?> AuthenticationRejected =
        LoggerMessage.Define<int>(
            LogLevel.Warning,
            new EventId(2101, "KeycloakAuthenticationRejected"),
            "Keycloak rejected administrative authentication. StatusCode={StatusCode}");

    private static readonly Action<ILogger, Exception?> AuthenticationFailed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2102, "KeycloakAuthenticationException"),
            "Keycloak administrative authentication failed before a token was obtained");

    private readonly KeycloakAdminOptions _options = options.Value;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private AccessToken? _cachedToken;

    public async ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var token = Volatile.Read(ref _cachedToken);
        if (IsUsable(token))
        {
            return token!.Value;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            token = Volatile.Read(ref _cachedToken);
            if (IsUsable(token))
            {
                return token!.Value;
            }

            token = await RequestTokenAsync(cancellationToken).ConfigureAwait(false);
            Volatile.Write(ref _cachedToken, token);
            return token.Value;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void Invalidate() => Volatile.Write(ref _cachedToken, null);

    public void Dispose() => _refreshLock.Dispose();

    private bool IsUsable(AccessToken? token) =>
        token is not null && timeProvider.GetUtcNow() < token.RefreshAtUtc;

    private async Task<AccessToken> RequestTokenAsync(CancellationToken cancellationToken)
    {
        using var activity = _options.Observability.Enabled
            ? KeycloakTelemetry.ActivitySource.StartActivity("keycloak.authentication", ActivityKind.Client)
            : null;
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildTokenEndpoint())
        {
            Content = BuildRequestContent(),
        };

        HttpResponseMessage response;
        try
        {
            var client = httpClientFactory.CreateClient(KeycloakHttpClientNames.TokenEndpoint);
            response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Authentication transport failure.");
            activity?.SetTag("exception.type", exception.GetType().FullName);
            if (_options.Observability.Enabled) AuthenticationFailed(logger, exception);
            throw;
        }

        using (response)
        {
            activity?.SetTag("http.response.status_code", (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Authentication rejected.");
                if (_options.Observability.Enabled) AuthenticationRejected(logger, (int)response.StatusCode, null);
                throw new KeycloakAuthenticationException(
                    $"Keycloak rejected the administrative authentication request with HTTP {(int)response.StatusCode}.",
                    response.StatusCode);
            }

            var payload = await response.Content
                .ReadFromJsonAsync<TokenResponse>(cancellationToken)
                .ConfigureAwait(false);

            if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken) || payload.ExpiresIn <= 0)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Invalid authentication response.");
                throw new KeycloakAuthenticationException(
                    "Keycloak returned an invalid administrative token response.");
            }

            if (!string.IsNullOrWhiteSpace(payload.TokenType) &&
                !string.Equals(payload.TokenType, "Bearer", StringComparison.OrdinalIgnoreCase))
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Unsupported authentication token type.");
                throw new KeycloakAuthenticationException(
                    "Keycloak returned an unsupported administrative token type.");
            }

            var lifetime = TimeSpan.FromSeconds(payload.ExpiresIn);
            var effectiveSkew = _options.Authentication.TokenRefreshSkew < lifetime / 2
                ? _options.Authentication.TokenRefreshSkew
                : lifetime / 2;
            var refreshAtUtc = timeProvider.GetUtcNow() + lifetime - effectiveSkew;

            activity?.SetStatus(ActivityStatusCode.Ok);
            return new AccessToken(payload.AccessToken, refreshAtUtc);
        }
    }

    private string BuildTokenEndpoint()
    {
        var realm = Uri.EscapeDataString(_options.AuthenticationRealm);
        return FormattableString.Invariant($"realms/{realm}/protocol/openid-connect/token");
    }

    private FormUrlEncodedContent BuildRequestContent()
    {
        var fields = _options.Authentication.Flow switch
        {
            KeycloakAuthenticationFlow.ClientCredentials => BuildClientCredentialsFields(),
            KeycloakAuthenticationFlow.Password => BuildPasswordFields(),
            _ => throw new KeycloakAuthenticationException("The configured authentication flow is unsupported."),
        };

        return new FormUrlEncodedContent(fields);
    }

    private List<KeyValuePair<string, string>> BuildClientCredentialsFields()
    {
        var credentials = _options.Authentication.ClientCredentials;
        var fields = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", credentials.ClientId),
            new("client_secret", credentials.ClientSecret),
        };

        AddScopes(fields, credentials.Scopes);
        return fields;
    }

    private List<KeyValuePair<string, string>> BuildPasswordFields()
    {
        var credentials = _options.Authentication.Password;
        var fields = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("client_id", credentials.ClientId),
            new("username", credentials.Username),
            new("password", credentials.Password),
        };

        if (!string.IsNullOrWhiteSpace(credentials.ClientSecret))
        {
            fields.Add(new("client_secret", credentials.ClientSecret));
        }

        AddScopes(fields, credentials.Scopes);
        return fields;
    }

    private static void AddScopes(
        List<KeyValuePair<string, string>> fields,
        IReadOnlyCollection<string> scopes)
    {
        if (scopes.Count > 0)
        {
            fields.Add(new("scope", string.Join(' ', scopes)));
        }
    }
}
