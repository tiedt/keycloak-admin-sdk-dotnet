using System.Net;

namespace Keycloak.AdminSdk.Exceptions;

/// <summary>Represents a Keycloak resource that could not be found.</summary>
public sealed class KeycloakNotFoundException(string message)
    : KeycloakApiException(message, HttpStatusCode.NotFound);
