using System.Net;

namespace Keycloak.AdminSdk.Observability;

internal static class KeycloakDiagnosticAdvisor
{
    public static (string Cause, string Remediation) ForStatus(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => ("Keycloak rejected the request representation or one of its fields.", "Validate required fields, identifiers, enum values, and the target Keycloak version."),
        HttpStatusCode.Unauthorized => ("The administrative access token is missing, expired, or was issued with invalid credentials.", "Verify AuthenticationRealm, ClientId, ClientSecret or password credentials, then confirm the client can obtain a token."),
        HttpStatusCode.Forbidden => ("The authenticated client does not have permission for this administrative operation.", "Grant only the required realm-management roles to the service account and retry."),
        HttpStatusCode.NotFound => ("The Realm or resource identifier does not exist in the selected context.", "Verify the Realm context and refresh resource identifiers before retrying."),
        HttpStatusCode.Conflict => ("A resource with the same unique name already exists or current state prevents the operation.", "Use a unique name or query the existing resource and apply an idempotent update."),
        HttpStatusCode.TooManyRequests => ("Keycloak or an upstream gateway applied rate limiting.", "Reduce concurrency, honor Retry-After, and review gateway or Keycloak capacity."),
        >= HttpStatusCode.InternalServerError => ("Keycloak or an upstream dependency failed while processing the request.", "Inspect Keycloak server, database, and reverse-proxy logs using the emitted TraceId."),
        _ => ("Keycloak returned an unsuccessful HTTP response.", "Inspect the status code, TraceId, target Realm, and server logs."),
    };

    public static (string Cause, string Remediation) ForException(Exception exception) => exception switch
    {
        OperationCanceledException => ("The request was canceled or exceeded its configured timeout.", "Check cancellation propagation and increase AttemptTimeout or TotalRequestTimeout only when justified."),
        HttpRequestException => ("The SDK could not complete the network connection to Keycloak.", "Verify DNS, URL, TLS certificates, proxy rules, firewall access, and Keycloak availability."),
        _ => ("An unexpected client-side failure occurred while calling Keycloak.", "Inspect the exception type, stack trace, TraceId, and application configuration."),
    };
}
