using Microsoft.Extensions.Options;

namespace Keycloak.AdminSdk.Configuration;

internal sealed class KeycloakAdminOptionsValidator : IValidateOptions<KeycloakAdminOptions>
{
    public ValidateOptionsResult Validate(string? name, KeycloakAdminOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();
        ValidateServer(options, failures);
        ValidateAuthentication(options, failures);
        ValidateResilience(options.Resilience, failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateServer(KeycloakAdminOptions options, List<string> failures)
    {
        if (options.ServerUrl is null || !options.ServerUrl.IsAbsoluteUri)
        {
            failures.Add("ServerUrl must be an absolute URI.");
        }
        else if (options.ServerUrl.Scheme is not ("http" or "https"))
        {
            failures.Add("ServerUrl must use HTTP or HTTPS.");
        }
        else if (!string.IsNullOrEmpty(options.ServerUrl.Query) ||
                 !string.IsNullOrEmpty(options.ServerUrl.Fragment) ||
                 !string.IsNullOrEmpty(options.ServerUrl.UserInfo))
        {
            failures.Add("ServerUrl cannot contain credentials, a query, or a fragment.");
        }

        if (string.IsNullOrWhiteSpace(options.AuthenticationRealm))
        {
            failures.Add("AuthenticationRealm is required.");
        }

        if (options.DefaultRealm is not null && string.IsNullOrWhiteSpace(options.DefaultRealm))
        {
            failures.Add("DefaultRealm cannot be empty when specified.");
        }
    }

    private static void ValidateAuthentication(
        KeycloakAdminOptions options,
        List<string> failures)
    {
        var authentication = options.Authentication;
        if (authentication.TokenRefreshSkew < TimeSpan.Zero)
        {
            failures.Add("Authentication.TokenRefreshSkew cannot be negative.");
        }

        switch (authentication.Flow)
        {
            case KeycloakAuthenticationFlow.ClientCredentials:
                if (string.IsNullOrWhiteSpace(authentication.ClientCredentials.ClientId))
                {
                    failures.Add("Authentication.ClientCredentials.ClientId is required.");
                }

                if (string.IsNullOrWhiteSpace(authentication.ClientCredentials.ClientSecret))
                {
                    failures.Add("Authentication.ClientCredentials.ClientSecret is required.");
                }

                break;

            case KeycloakAuthenticationFlow.Password:
                ValidatePasswordFlow(authentication.Password, failures);
                break;

            default:
                failures.Add("Authentication.Flow is unsupported.");
                break;
        }
    }

    private static void ValidatePasswordFlow(
        PasswordCredentialsOptions password,
        List<string> failures)
    {
        if (!password.AllowPasswordGrant)
        {
            failures.Add("Authentication.Password.AllowPasswordGrant must be explicitly enabled.");
        }

        if (string.IsNullOrWhiteSpace(password.ClientId))
        {
            failures.Add("Authentication.Password.ClientId is required.");
        }

        if (string.IsNullOrWhiteSpace(password.Username))
        {
            failures.Add("Authentication.Password.Username is required.");
        }

        if (string.IsNullOrWhiteSpace(password.Password))
        {
            failures.Add("Authentication.Password.Password is required.");
        }
    }

    private static void ValidateResilience(
        KeycloakResilienceOptions resilience,
        List<string> failures)
    {
        if (resilience.MaxRetryAttempts is < 1 or > 10)
        {
            failures.Add("Resilience.MaxRetryAttempts must be between 1 and 10.");
        }

        if (resilience.RetryBaseDelay <= TimeSpan.Zero)
        {
            failures.Add("Resilience.RetryBaseDelay must be greater than zero.");
        }

        if (resilience.AttemptTimeout <= TimeSpan.Zero)
        {
            failures.Add("Resilience.AttemptTimeout must be greater than zero.");
        }

        if (resilience.TotalRequestTimeout < resilience.AttemptTimeout)
        {
            failures.Add("Resilience.TotalRequestTimeout cannot be shorter than AttemptTimeout.");
        }
    }
}
