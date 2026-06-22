using System.Net;

namespace Keycloak.AdminSdk.Exceptions;

/// <summary>Represents an administrative operation that is not authorized.</summary>
public sealed class KeycloakAuthorizationException(string message, HttpStatusCode statusCode)
    : KeycloakApiException(message, statusCode);
