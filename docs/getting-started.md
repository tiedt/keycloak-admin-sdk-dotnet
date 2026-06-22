# Instalação e primeiro uso

## 1. Instalar o pacote

Após publicação no NuGet.org:

```powershell
dotnet add package Keycloak.AdminSdk --version 0.1.0-preview.1
```

Para testar o pacote gerado localmente neste repositório:

```powershell
dotnet add package Keycloak.AdminSdk `
  --version 0.1.0-preview.1 `
  --source C:\projetos\keycloak\artifacts
```

## 2. Preparar um Client administrativo no Keycloak

No Realm usado para autenticação, geralmente `master`:

1. Crie um Client OpenID Connect, por exemplo `my-application-admin`.
2. Habilite `Client authentication`.
3. Habilite `Service accounts roles`.
4. Copie o Client Secret para um secret manager.
5. Em `Service account roles`, atribua apenas as roles necessárias do Client `realm-management`.

Exemplos de permissões:

- Users: `query-users`, `view-users`, `manage-users`.
- Clients e Client Scopes: `query-clients`, `view-clients`, `manage-clients`.
- Realms: `view-realm`, `manage-realm`.
- Groups e Roles: normalmente `view-users`, `manage-users` e permissões específicas adotadas pela instalação.

Evite `realm-admin` quando permissões menores forem suficientes.

## 3. Configurar `appsettings.json`

```json
{
  "Keycloak": {
    "Admin": {
      "ServerUrl": "https://identity.example.com",
      "DefaultRealm": "customers",
      "AuthenticationRealm": "master",
      "Authentication": {
        "Flow": "ClientCredentials",
        "TokenRefreshSkew": "00:00:30",
        "ClientCredentials": {
          "ClientId": "my-application-admin",
          "ClientSecret": "DO-NOT-COMMIT-SECRETS"
        }
      }
    }
  }
}
```

Substitua o secret em produção por variável de ambiente, Azure Key Vault, AWS Secrets Manager, Kubernetes Secret ou mecanismo equivalente.

## 4. Registrar no `Program.cs`

```csharp
using Keycloak.AdminSdk.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeycloakAdmin(
    builder.Configuration.GetSection(KeycloakAdminOptions.SectionName));
```

Também é possível configurar por delegate:

```csharp
builder.Services.AddKeycloakAdmin(options =>
{
    options.ServerUrl = new Uri("https://identity.example.com");
    options.DefaultRealm = "customers";
    options.AuthenticationRealm = "master";
    options.Authentication.ClientCredentials.ClientId = "my-application-admin";
    options.Authentication.ClientCredentials.ClientSecret = secret;
});
```

## 5. Injetar e utilizar

```csharp
using Keycloak.AdminSdk.Abstractions;
using Keycloak.AdminSdk.Features.Users.Models;

public sealed class UserProvisioner(IKeycloakAdminClient keycloak)
{
    public async Task ProvisionAsync(CancellationToken cancellationToken)
    {
        var realm = keycloak.ForRealm("customers");

        var user = await realm.Users.CreateAsync(
            new CreateUserRequest("alice")
            {
                Email = "alice@example.com",
                FirstName = "Alice",
                Enabled = true,
            },
            cancellationToken);

        await realm.Users.ResetPasswordAsync(
            user.Id,
            new SetPasswordRequest("temporary-password") { Temporary = true },
            cancellationToken);
    }
}
```

## 6. Usar serviços diretamente pela DI

Quando `DefaultRealm` estiver configurado, é possível injetar interfaces setoriais:

```csharp
public sealed class UserReader(IKeycloakUserService users)
{
    public Task<UserResource?> FindAsync(string username, CancellationToken cancellationToken) =>
        users.FindByUsernameAsync(username, cancellationToken);
}
```

Sem `DefaultRealm`, utilize `IKeycloakAdminClient.ForRealm(...)` para evitar contexto ambíguo.

## 7. Executar a aplicação de referência

O projeto em `samples/Keycloak.AdminSdk.SampleHost` demonstra uma aplicação ASP.NET Core completa com JWT Bearer, policies por scope, rate limiting, tratamento de exceções com Problem Details, health check e OpenTelemetry opcional.

```powershell
dotnet user-secrets set "Keycloak:Admin:Authentication:ClientCredentials:ClientSecret" "seu-secret" --project samples/Keycloak.AdminSdk.SampleHost
dotnet run --project samples/Keycloak.AdminSdk.SampleHost
```

Use o Sample Host como referência arquitetural. Antes de expor operações administrativas, adapte autorização, auditoria, idempotência e limites ao seu domínio.
