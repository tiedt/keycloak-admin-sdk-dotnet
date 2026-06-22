using System.Net;

namespace Keycloak.AdminSdk.Exceptions;

/// <summary>Represents a conflict with existing Keycloak state.</summary>
public sealed class KeycloakConflictException(string message)
    : KeycloakApiException(message, HttpStatusCode.Conflict);
