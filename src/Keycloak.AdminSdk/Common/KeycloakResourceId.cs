namespace Keycloak.AdminSdk.Common;

/// <summary>Represents an opaque resource identifier assigned by Keycloak.</summary>
public readonly record struct KeycloakResourceId
{
    /// <summary>Initializes a Keycloak resource identifier.</summary>
    public KeycloakResourceId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>Gets the identifier value.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
