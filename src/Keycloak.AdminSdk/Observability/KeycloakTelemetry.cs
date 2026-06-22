using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Keycloak.AdminSdk.Observability;

/// <summary>Names and instrumentation sources used by OpenTelemetry consumers.</summary>
public static class KeycloakTelemetry
{
    public const string ActivitySourceName = "Keycloak.AdminSdk";
    public const string MeterName = "Keycloak.AdminSdk";

    internal static ActivitySource ActivitySource { get; } = new(ActivitySourceName);
    internal static Meter Meter { get; } = new(MeterName);
    internal static Counter<long> Requests { get; } = Meter.CreateCounter<long>("keycloak.admin.requests");
    internal static Counter<long> Errors { get; } = Meter.CreateCounter<long>("keycloak.admin.errors");
    internal static Histogram<double> Duration { get; } = Meter.CreateHistogram<double>("keycloak.admin.request.duration", "ms");
}
