using Keycloak.AdminSdk.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Keycloak.AdminSdk.SampleHost.Infrastructure;

internal sealed class KeycloakExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<KeycloakExceptionHandler> logger) : IExceptionHandler
{
    private static readonly Action<ILogger, string, Exception?> LogKeycloakFailure =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, "KeycloakAdministrationFailure"),
            "Keycloak administration failed. TraceId={TraceId}");

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not KeycloakApiException and not KeycloakAuthenticationException)
        {
            return false;
        }

        LogKeycloakFailure(logger, httpContext.TraceIdentifier, exception);

        var status = exception switch
        {
            KeycloakValidationException => StatusCodes.Status400BadRequest,
            KeycloakAuthenticationException => StatusCodes.Status502BadGateway,
            KeycloakAuthorizationException => StatusCodes.Status403Forbidden,
            KeycloakNotFoundException => StatusCodes.Status404NotFound,
            KeycloakConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status502BadGateway,
        };

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = "Keycloak administration operation failed",
                Detail = exception.Message,
                Extensions = { ["traceId"] = httpContext.TraceIdentifier },
            },
        });
    }
}
