namespace Keycloak.AdminSdk.Common;

/// <summary>Identifies a Keycloak realm.</summary>
public readonly record struct RealmName
{
    /// <summary>Initializes a realm identifier.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
    public RealmName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>Gets the realm name.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Converts a string to a realm name.</summary>
    public static implicit operator RealmName(string value) => new(value);

    /// <summary>Converts a realm name to its string representation.</summary>
    public static implicit operator string(RealmName realm) => realm.Value;
}
