using System.Text.Json.Serialization;

namespace Keycloak.AdminSdk.Internal.Representations;

internal sealed class RealmRepresentation
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("realm")]
    public string? Realm { get; init; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; init; }

    [JsonPropertyName("displayNameHtml")]
    public string? DisplayNameHtml { get; init; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; init; }

    [JsonPropertyName("registrationAllowed")]
    public bool? RegistrationAllowed { get; init; }

    [JsonPropertyName("registrationEmailAsUsername")]
    public bool? RegistrationEmailAsUsername { get; init; }

    [JsonPropertyName("rememberMe")]
    public bool? RememberMe { get; init; }

    [JsonPropertyName("verifyEmail")]
    public bool? VerifyEmail { get; init; }

    [JsonPropertyName("loginWithEmailAllowed")]
    public bool? LoginWithEmailAllowed { get; init; }

    [JsonPropertyName("duplicateEmailsAllowed")]
    public bool? DuplicateEmailsAllowed { get; init; }

    [JsonPropertyName("resetPasswordAllowed")]
    public bool? ResetPasswordAllowed { get; init; }

    [JsonPropertyName("editUsernameAllowed")]
    public bool? EditUsernameAllowed { get; init; }

    [JsonPropertyName("bruteForceProtected")]
    public bool? BruteForceProtected { get; init; }

    [JsonPropertyName("sslRequired")]
    public string? SslRequired { get; init; }
}
