using System.Diagnostics;
using Keycloak.AdminSdk.Abstractions.Http;
using Keycloak.AdminSdk.Configuration;
using Keycloak.AdminSdk.Observability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Keycloak.AdminSdk.Http;

internal sealed class KeycloakHttpClient(
    IHttpClientFactory httpClientFactory,
    IOptions<KeycloakAdminOptions> options,
    IEnumerable<IKeycloakDiagnosticSink> diagnosticSinks,
    ILogger<KeycloakHttpClient> logger,
    TimeProvider timeProvider) : IKeycloakHttpClient
{
    private static readonly Action<ILogger, string, string?, int, double, Exception?> RequestFailed =
        LoggerMessage.Define<string, string?, int, double>(
            LogLevel.Warning,
            new EventId(2001, "KeycloakRequestFailed"),
            "Keycloak Admin request failed. Method={HttpMethod} Path={RequestPath} StatusCode={StatusCode} DurationMs={DurationMs}");

    private static readonly Action<ILogger, string, string?, int, double, Exception?> ServerRequestFailed =
        LoggerMessage.Define<string, string?, int, double>(
            LogLevel.Error,
            new EventId(2002, "KeycloakServerRequestFailed"),
            "Keycloak Admin server failure. Method={HttpMethod} Path={RequestPath} StatusCode={StatusCode} DurationMs={DurationMs}");

    private static readonly Action<ILogger, string, string?, double, Exception?> RequestSucceeded =
        LoggerMessage.Define<string, string?, double>(
            LogLevel.Debug,
            new EventId(2000, "KeycloakRequestSucceeded"),
            "Keycloak Admin request succeeded. Method={HttpMethod} Path={RequestPath} DurationMs={DurationMs}");

    private static readonly Action<ILogger, string, string?, double, Exception?> RequestThrew =
        LoggerMessage.Define<string, string?, double>(
            LogLevel.Error,
            new EventId(2003, "KeycloakRequestException"),
            "Keycloak Admin request raised an exception. Method={HttpMethod} Path={RequestPath} DurationMs={DurationMs}");

    private static readonly Action<ILogger, string, Exception?> DiagnosticSinkFailed =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2004, "KeycloakDiagnosticSinkFailed"),
            "Keycloak diagnostic sink {SinkType} failed and was ignored");

    private readonly KeycloakObservabilityOptions _observability = options.Value.Observability;
    private readonly IReadOnlyCollection<IKeycloakDiagnosticSink> _diagnosticSinks = diagnosticSinks.ToArray();

    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var method = request.Method.Method;
        var path = _observability.IncludeRequestPath ? GetSanitizedPath(request.RequestUri) : null;
        var started = timeProvider.GetTimestamp();
        using var activity = StartActivity(method, path);

        try
        {
            var client = httpClientFactory.CreateClient(KeycloakHttpClientNames.AdminApi);
            var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            var duration = timeProvider.GetElapsedTime(started).TotalMilliseconds;
            RecordRequest(method, response.StatusCode, duration);

            activity?.SetTag("http.response.status_code", (int)response.StatusCode);
            if (response.IsSuccessStatusCode)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                if (_observability.Enabled && _observability.LogSuccessfulRequests)
                {
                    RequestSucceeded(logger, method, path, duration, null);
                }

                return response;
            }

            activity?.SetStatus(ActivityStatusCode.Error, "Keycloak returned an unsuccessful response.");
            var advice = KeycloakDiagnosticAdvisor.ForStatus(response.StatusCode);
            activity?.SetTag("keycloak.error.cause_hint", advice.Cause);
            activity?.SetTag("keycloak.error.remediation_hint", advice.Remediation);

            if (_observability.Enabled)
            {
                var log = (int)response.StatusCode >= 500 ? ServerRequestFailed : RequestFailed;
                log(logger, method, path, (int)response.StatusCode, duration, null);
                await PublishDiagnosticAsync(
                    CreateDiagnostic(method, path, (int)response.StatusCode, null, advice, activity),
                    cancellationToken).ConfigureAwait(false);
            }

            return response;
        }
        catch (Exception exception)
        {
            var duration = timeProvider.GetElapsedTime(started).TotalMilliseconds;
            RecordException(method, duration);
            activity?.SetStatus(ActivityStatusCode.Error, "Keycloak request raised an exception.");
            activity?.SetTag("exception.type", exception.GetType().FullName);
            var advice = KeycloakDiagnosticAdvisor.ForException(exception);
            activity?.SetTag("keycloak.error.cause_hint", advice.Cause);
            activity?.SetTag("keycloak.error.remediation_hint", advice.Remediation);

            if (_observability.Enabled)
            {
                RequestThrew(logger, method, path, duration, exception);
                await PublishDiagnosticAsync(
                    CreateDiagnostic(method, path, null, exception.GetType().FullName, advice, activity),
                    CancellationToken.None).ConfigureAwait(false);
            }

            throw;
        }
    }

    private Activity? StartActivity(string method, string? path)
    {
        if (!_observability.Enabled)
        {
            return null;
        }

        var activity = KeycloakTelemetry.ActivitySource.StartActivity("keycloak.admin.request", ActivityKind.Client);
        activity?.SetTag("http.request.method", method);
        activity?.SetTag("url.path", path);
        activity?.SetTag("server.address", "keycloak");
        return activity;
    }

    private void RecordRequest(string method, System.Net.HttpStatusCode statusCode, double duration)
    {
        if (!_observability.Enabled) return;
        var tags = new TagList { { "http.request.method", method }, { "http.response.status_code", (int)statusCode } };
        KeycloakTelemetry.Requests.Add(1, tags);
        KeycloakTelemetry.Duration.Record(duration, tags);
        if ((int)statusCode >= 400) KeycloakTelemetry.Errors.Add(1, tags);
    }

    private void RecordException(string method, double duration)
    {
        if (!_observability.Enabled) return;
        var tags = new TagList { { "http.request.method", method }, { "error.type", "exception" } };
        KeycloakTelemetry.Requests.Add(1, tags);
        KeycloakTelemetry.Errors.Add(1, tags);
        KeycloakTelemetry.Duration.Record(duration, tags);
    }

    private async ValueTask PublishDiagnosticAsync(KeycloakDiagnosticEvent diagnostic, CancellationToken token)
    {
        if (!_observability.PublishDiagnosticEvents) return;
        foreach (var sink in _diagnosticSinks)
        {
            try
            {
                await sink.PublishAsync(diagnostic, token).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                DiagnosticSinkFailed(logger, sink.GetType().FullName ?? sink.GetType().Name, exception);
            }
        }
    }

    private KeycloakDiagnosticEvent CreateDiagnostic(
        string method,
        string? path,
        int? statusCode,
        string? exceptionType,
        (string Cause, string Remediation) advice,
        Activity? activity) => new()
        {
            TimestampUtc = timeProvider.GetUtcNow(),
            Category = "Keycloak.AdminSdk.Http",
            HttpMethod = method,
            RequestPath = path,
            HttpStatusCode = statusCode,
            ExceptionType = exceptionType,
            CauseHint = _observability.EmitDiagnosticHints ? advice.Cause : string.Empty,
            RemediationHint = _observability.EmitDiagnosticHints ? advice.Remediation : string.Empty,
            TraceId = (activity ?? Activity.Current)?.TraceId.ToString(),
        };

    private static string? GetSanitizedPath(Uri? uri)
    {
        if (uri is null) return null;
        var path = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString.Split('?', 2)[0];
        return Uri.UnescapeDataString(path);
    }
}
