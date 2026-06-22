namespace Keycloak.AdminSdk.Authentication;

internal interface IKeycloakTokenProvider
{
    ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken);

    void Invalidate();
}
