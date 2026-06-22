using System.Net;
using System.Net.Http.Json;
using Keycloak.AdminSdk.Exceptions;
using Keycloak.AdminSdk.Internal.Serialization;

namespace Keycloak.AdminSdk.Internal.Http;

internal static class KeycloakResponse
{
    public static void EnsureSuccess(HttpResponseMessage response, string operation)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = $"Keycloak operation '{operation}' failed with HTTP {(int)response.StatusCode}.";
        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new KeycloakValidationException(message),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                new KeycloakAuthorizationException(message, response.StatusCode),
            HttpStatusCode.NotFound => new KeycloakNotFoundException(message),
            HttpStatusCode.Conflict => new KeycloakConflictException(message),
            _ => new KeycloakApiException(message, response.StatusCode),
        };
    }

    public static async Task<T> ReadRequiredAsync<T>(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        EnsureSuccess(response, operation);

        try
        {
            var value = await response.Content
                .ReadFromJsonAsync<T>(KeycloakJson.Options, cancellationToken)
                .ConfigureAwait(false);

            return value ?? throw new KeycloakApiException(
                $"Keycloak operation '{operation}' returned an empty response.",
                response.StatusCode);
        }
        catch (KeycloakApiException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new KeycloakApiException(
                $"Keycloak operation '{operation}' returned an invalid response.",
                response.StatusCode,
                exception);
        }
    }
}
