namespace Keycloak.AdminSdk.Abstractions.Http;

/// <summary>Sends authenticated requests to the Keycloak Admin REST API.</summary>
public interface IKeycloakHttpClient
{
    /// <summary>Sends an HTTP request using the configured authentication and resilience pipeline.</summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The response. The caller is responsible for disposing it.</returns>
    Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default);
}
