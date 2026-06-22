using System.Net;
using System.Net.Http.Headers;

namespace Keycloak.AdminSdk.Authentication;

internal sealed class AuthenticationDelegatingHandler(IKeycloakTokenProvider tokenProvider)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            tokenProvider.Invalidate();
        }

        return response;
    }
}
