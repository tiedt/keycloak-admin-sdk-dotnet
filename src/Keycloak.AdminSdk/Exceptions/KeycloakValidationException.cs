using System.Net;

namespace Keycloak.AdminSdk.Exceptions;

/// <summary>Represents a request rejected by Keycloak as invalid.</summary>
public sealed class KeycloakValidationException(string message)
    : KeycloakApiException(message, HttpStatusCode.BadRequest);
