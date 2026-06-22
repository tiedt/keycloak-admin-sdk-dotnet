using Keycloak.AdminSdk.Abstractions;

namespace Keycloak.AdminSdk.ArchitectureTests;

public sealed class PublicApiArchitectureTests
{
    [Fact]
    public void ContextImplementationsAreNotPublic()
    {
        var assembly = typeof(IKeycloakAdminClient).Assembly;

        var exportedImplementations = assembly
            .GetExportedTypes()
            .Where(type => string.Equals(
                type.Namespace,
                "Keycloak.AdminSdk.Context",
                StringComparison.Ordinal));

        Assert.Empty(exportedImplementations);
    }

    [Fact]
    public void ServiceImplementationsAreNotPublic()
    {
        var assembly = typeof(IKeycloakAdminClient).Assembly;
        var exposedImplementations = assembly.GetExportedTypes()
            .Where(type => type.IsClass)
            .Where(type => type.GetInterfaces().Any(contract =>
                contract.Namespace?.StartsWith("Keycloak.AdminSdk.Abstractions", StringComparison.Ordinal) == true));

        Assert.Empty(exposedImplementations);
    }
}
