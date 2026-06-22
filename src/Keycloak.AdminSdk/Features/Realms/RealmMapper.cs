using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Exceptions;
using Keycloak.AdminSdk.Features.Realms.Models;
using Keycloak.AdminSdk.Internal.Representations;

namespace Keycloak.AdminSdk.Features.Realms;

internal static class RealmMapper
{
    public static RealmResource ToResource(RealmRepresentation representation)
    {
        if (string.IsNullOrWhiteSpace(representation.Realm))
        {
            throw new KeycloakApiException("Keycloak returned a realm without a name.");
        }

        return new RealmResource
        {
            Id = string.IsNullOrWhiteSpace(representation.Id)
                ? null
                : new KeycloakResourceId(representation.Id),
            Name = new RealmName(representation.Realm),
            DisplayName = representation.DisplayName,
            DisplayNameHtml = representation.DisplayNameHtml,
            Enabled = representation.Enabled ?? false,
            RegistrationAllowed = representation.RegistrationAllowed ?? false,
            RegistrationEmailAsUsername = representation.RegistrationEmailAsUsername ?? false,
            RememberMe = representation.RememberMe ?? false,
            VerifyEmail = representation.VerifyEmail ?? false,
            LoginWithEmailAllowed = representation.LoginWithEmailAllowed ?? false,
            DuplicateEmailsAllowed = representation.DuplicateEmailsAllowed ?? false,
            ResetPasswordAllowed = representation.ResetPasswordAllowed ?? false,
            EditUsernameAllowed = representation.EditUsernameAllowed ?? false,
            BruteForceProtected = representation.BruteForceProtected ?? false,
            SslRequired = FromWireValue(representation.SslRequired),
        };
    }

    public static RealmRepresentation ToRepresentation(CreateRealmRequest request) => new()
    {
        Realm = request.Name.Value,
        DisplayName = request.DisplayName,
        Enabled = request.Enabled,
        RegistrationAllowed = request.RegistrationAllowed,
        RegistrationEmailAsUsername = request.RegistrationEmailAsUsername,
        RememberMe = request.RememberMe,
        VerifyEmail = request.VerifyEmail,
        LoginWithEmailAllowed = request.LoginWithEmailAllowed,
        DuplicateEmailsAllowed = request.DuplicateEmailsAllowed,
        ResetPasswordAllowed = request.ResetPasswordAllowed,
        EditUsernameAllowed = request.EditUsernameAllowed,
        BruteForceProtected = request.BruteForceProtected,
        SslRequired = ToWireValue(request.SslRequired),
    };

    public static RealmRepresentation ToRepresentation(UpdateRealmRequest request) => new()
    {
        DisplayName = request.DisplayName,
        Enabled = request.Enabled,
        RegistrationAllowed = request.RegistrationAllowed,
        RegistrationEmailAsUsername = request.RegistrationEmailAsUsername,
        RememberMe = request.RememberMe,
        VerifyEmail = request.VerifyEmail,
        LoginWithEmailAllowed = request.LoginWithEmailAllowed,
        DuplicateEmailsAllowed = request.DuplicateEmailsAllowed,
        ResetPasswordAllowed = request.ResetPasswordAllowed,
        EditUsernameAllowed = request.EditUsernameAllowed,
        BruteForceProtected = request.BruteForceProtected,
        SslRequired = request.SslRequired is null ? null : ToWireValue(request.SslRequired.Value),
    };

    private static SslRequirement FromWireValue(string? value) => value switch
    {
        "none" => SslRequirement.None,
        "external" => SslRequirement.External,
        "all" => SslRequirement.All,
        _ => SslRequirement.Unknown,
    };

    private static string ToWireValue(SslRequirement value) => value switch
    {
        SslRequirement.None => "none",
        SslRequirement.External => "external",
        SslRequirement.All => "all",
        _ => throw new ArgumentOutOfRangeException(
            nameof(value),
            value,
            "An explicit SSL requirement is required for write operations."),
    };
}
