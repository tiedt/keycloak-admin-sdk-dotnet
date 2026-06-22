using Keycloak.AdminSdk.Configuration;

namespace Keycloak.AdminSdk.UnitTests.Configuration;

public sealed class KeycloakAdminOptionsValidatorTests
{
    private readonly KeycloakAdminOptionsValidator _validator = new();

    [Fact]
    public void ValidClientCredentialsConfigurationSucceeds()
    {
        var result = _validator.Validate(null, CreateValidOptions());

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void PasswordFlowRequiresExplicitOptIn()
    {
        var options = CreateValidOptions();
        options.Authentication.Flow = KeycloakAuthenticationFlow.Password;
        options.Authentication.Password = new PasswordCredentialsOptions
        {
            ClientId = "admin-cli",
            Username = "admin",
            Password = "secret",
        };

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(
            result.Failures,
            failure => failure.Contains("AllowPasswordGrant", StringComparison.Ordinal));
    }

    [Fact]
    public void ServerUrlRejectsEmbeddedCredentials()
    {
        var options = CreateValidOptions();
        options.ServerUrl = new Uri("https://admin:secret@identity.example/");

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
    }

    [Fact]
    public void DefaultRealmRejectsWhitespaceWhenSpecified()
    {
        var options = CreateValidOptions();
        options.DefaultRealm = " ";

        var result = _validator.Validate(null, options);

        Assert.True(result.Failed);
    }

    private static KeycloakAdminOptions CreateValidOptions() => new()
    {
        ServerUrl = new Uri("https://identity.example/"),
        AuthenticationRealm = "master",
        Authentication = new KeycloakAuthenticationOptions
        {
            ClientCredentials = new ClientCredentialsOptions
            {
                ClientId = "admin-client",
                ClientSecret = "secret",
            },
        },
    };
}
