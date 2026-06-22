using System.Net;

namespace Keycloak.AdminSdk.Exceptions;

/// <summary>Represents a failure while obtaining an administrative access token.</summary>
public sealed class KeycloakAuthenticationException : Exception
{
    /// <summary>Initializes an authentication exception.</summary>
    public KeycloakAuthenticationException(string message, HttpStatusCode? statusCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>Gets the HTTP status returned by Keycloak, when available.</summary>
    public HttpStatusCode? StatusCode { get; }
}
