namespace Keycloak.AdminSdk.Exceptions;

/// <summary>Represents a multi-step provisioning plan that failed after it started.</summary>
public sealed class KeycloakProvisioningException(string message, Exception innerException)
    : Exception(message, innerException);
