namespace Keycloak.AdminSdk.IntegrationTests;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class KeycloakIntegrationFactAttribute : FactAttribute
{
    public KeycloakIntegrationFactAttribute()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("KEYCLOAK_INTEGRATION_TESTS"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Skip = "Set KEYCLOAK_INTEGRATION_TESTS=true to run tests against the local Keycloak instance.";
        }
    }
}
