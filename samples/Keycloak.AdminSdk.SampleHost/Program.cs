using System.Threading.RateLimiting;
using Keycloak.AdminSdk.Configuration;
using Keycloak.AdminSdk.Observability;
using Keycloak.AdminSdk.SampleHost.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
var mcpEnabled = builder.Configuration.GetValue<bool>("Diagnostics:Mcp:Enabled");
var otlpEnabled = builder.Configuration.GetValue<bool>("Observability:Otlp:Enabled");

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<KeycloakExceptionHandler>();
builder.Services.AddHealthChecks();

builder.Services.AddKeycloakAdmin(
    builder.Configuration.GetSection(KeycloakAdminOptions.SectionName));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = "roles",
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(SecurityPolicies.KeycloakAdministration, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context => context.User.FindAll("scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Contains("keycloak.admin", StringComparer.Ordinal));
    });

    options.AddPolicy(SecurityPolicies.McpDiagnostics, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context => context.User.FindAll("scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Contains("keycloak.diagnostics", StringComparer.Ordinal));
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("keycloak-admin", limiter =>
    {
        limiter.PermitLimit = 30;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing =>
    {
        tracing.AddSource(KeycloakTelemetry.ActivitySourceName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
        if (otlpEnabled) tracing.AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddMeter(KeycloakTelemetry.MeterName);
        if (otlpEnabled) metrics.AddOtlpExporter();
    });

if (otlpEnabled)
{
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeScopes = true;
        logging.IncludeFormattedMessage = true;
        logging.AddOtlpExporter();
    });
}

if (mcpEnabled)
{
    builder.Services.AddKeycloakAdminDiagnosticsMcp(options =>
    {
        options.Capacity = 500;
        options.MaximumQuerySize = 100;
    });
}

var app = builder.Build();

app.UseExceptionHandler();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

if (mcpEnabled)
{
    app.MapMcp("/mcp/keycloak")
        .RequireAuthorization(SecurityPolicies.McpDiagnostics);
}

app.Run();

/// <summary>Entry point exposed for integration testing.</summary>
public partial class Program;
