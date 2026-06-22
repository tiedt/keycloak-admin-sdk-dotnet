using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.Features.Realms.Models;

/// <summary>Defines a realm to create.</summary>
public sealed record CreateRealmRequest(RealmName Name)
{
    /// <summary>Gets the human-readable realm name.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Gets whether the realm is enabled.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Gets whether self-registration is enabled.</summary>
    public bool RegistrationAllowed { get; init; }

    /// <summary>Gets whether an email address is used as the registration username.</summary>
    public bool RegistrationEmailAsUsername { get; init; }

    /// <summary>Gets whether users may request persistent login sessions.</summary>
    public bool RememberMe { get; init; }

    /// <summary>Gets whether email verification is required.</summary>
    public bool VerifyEmail { get; init; }

    /// <summary>Gets whether users may sign in using their email address.</summary>
    public bool LoginWithEmailAllowed { get; init; } = true;

    /// <summary>Gets whether duplicate email addresses are allowed.</summary>
    public bool DuplicateEmailsAllowed { get; init; }

    /// <summary>Gets whether users may reset their passwords.</summary>
    public bool ResetPasswordAllowed { get; init; }

    /// <summary>Gets whether usernames may be changed.</summary>
    public bool EditUsernameAllowed { get; init; }

    /// <summary>Gets whether brute-force detection is enabled.</summary>
    public bool BruteForceProtected { get; init; }

    /// <summary>Gets the realm HTTPS requirement.</summary>
    public SslRequirement SslRequired { get; init; } = SslRequirement.External;
}
