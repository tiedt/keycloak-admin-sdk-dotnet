using System.Net;

namespace Keycloak.AdminSdk.Exceptions;

/// <summary>Represents a failed Keycloak Admin REST API operation.</summary>
public class KeycloakApiException : Exception
{
    /// <summary>Initializes an API exception.</summary>
    public KeycloakApiException(
        string message,
        HttpStatusCode? statusCode = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>Gets the HTTP status returned by Keycloak, when available.</summary>
    public HttpStatusCode? StatusCode { get; }
}
