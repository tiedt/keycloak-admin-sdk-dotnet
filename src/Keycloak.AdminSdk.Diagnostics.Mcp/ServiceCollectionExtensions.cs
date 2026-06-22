using Keycloak.AdminSdk.Observability;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers the optional read-only MCP diagnostics adapter.</summary>
public static class KeycloakMcpServiceCollectionExtensions
{
    public static IMcpServerBuilder AddKeycloakAdminDiagnosticsMcp(
        this IServiceCollection services,
        Action<Keycloak.AdminSdk.Diagnostics.Mcp.KeycloakMcpDiagnosticsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (configure is not null)
            services.AddOptions<Keycloak.AdminSdk.Diagnostics.Mcp.KeycloakMcpDiagnosticsOptions>().Configure(configure);
        else
            services.AddOptions<Keycloak.AdminSdk.Diagnostics.Mcp.KeycloakMcpDiagnosticsOptions>();

        services.TryAddSingleton<Keycloak.AdminSdk.Diagnostics.Mcp.InMemoryKeycloakDiagnosticStore>();
        services.TryAddSingleton<Keycloak.AdminSdk.Diagnostics.Mcp.IKeycloakDiagnosticReader>(
            provider => provider.GetRequiredService<Keycloak.AdminSdk.Diagnostics.Mcp.InMemoryKeycloakDiagnosticStore>());
        services.AddSingleton<IKeycloakDiagnosticSink>(
            provider => provider.GetRequiredService<Keycloak.AdminSdk.Diagnostics.Mcp.InMemoryKeycloakDiagnosticStore>());

        return services.AddMcpServer()
            .WithHttpTransport()
            .WithTools<Keycloak.AdminSdk.Diagnostics.Mcp.KeycloakDiagnosticTools>();
    }
}
