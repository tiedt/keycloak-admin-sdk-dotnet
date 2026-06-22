# Guia dos módulos

## Contexto de Realm

```csharp
var tenantA = keycloak.ForRealm("tenant-a");
var tenantB = keycloak.ForRealm("tenant-b");
```

Cada contexto é imutável e pode ser usado concorrentemente. Selecionar `tenant-b` não altera chamadas que utilizam `tenant-a`.

## Realms

O serviço global fica em `keycloak.Realms`:

```csharp
var realm = await keycloak.Realms.CreateAsync(new CreateRealmRequest("tenant-a")
{
    DisplayName = "Tenant A",
    Enabled = true,
    SslRequired = SslRequirement.All,
});

var all = await keycloak.Realms.GetAllAsync();
var found = await keycloak.Realms.FindAsync("tenant-a");

await keycloak.Realms.UpdateAsync(
    "tenant-a",
    new UpdateRealmRequest { RegistrationAllowed = true });

await keycloak.Realms.DeleteAsync("tenant-a");
```

## Users

```csharp
var user = await realm.Users.CreateAsync(new CreateUserRequest("alice")
{
    Email = "alice@example.com",
    FirstName = "Alice",
    LastName = "Silva",
    Attributes = new Dictionary<string, IReadOnlyCollection<string>>
    {
        ["department"] = ["engineering"],
    },
});

await realm.Users.ResetPasswordAsync(
    user.Id,
    new SetPasswordRequest("temporary-password") { Temporary = true });

var matches = await realm.Users.GetAllAsync(new UserQuery
{
    Search = "alice",
    First = 0,
    Max = 20,
});
```

## Clients

```csharp
var client = await realm.Clients.CreateAsync(new CreateClientRequest("orders-api")
{
    Name = "Orders API",
    ServiceAccountsEnabled = true,
    StandardFlowEnabled = false,
    DirectAccessGrantsEnabled = false,
});

var found = await realm.Clients.FindByClientIdAsync("orders-api");
```

## Client Scopes

```csharp
var scope = await realm.ClientScopes.CreateAsync(
    new CreateClientScopeRequest("orders.read")
    {
        Description = "Read access to orders",
    });

await realm.Clients.AddDefaultScopeAsync(client.Id, scope.Id);
await realm.Clients.RemoveDefaultScopeAsync(client.Id, scope.Id);
await realm.Clients.AddOptionalScopeAsync(client.Id, scope.Id);
```

## Provisionamento fluído de Client

```csharp
var client = await realm.Clients
    .Define(new CreateClientRequest("orders-api"))
    .WithDefaultScope(scope)
    .WithClientRole(new CreateRoleRequest("orders-reader"))
    .WithClientRole(new CreateRoleRequest("orders-writer"))
    .ApplyAsync(cancellationToken);
```

Se uma associação falhar, o Client recém-criado é removido. Use `.KeepClientOnFailure()` apenas quando o estado parcial for deliberadamente desejado.

## Realm Roles

```csharp
var role = await realm.Roles.CreateRealmRoleAsync(
    new CreateRoleRequest("operator")
    {
        Description = "Operates tenant resources",
    });

await realm.Roles.AssignRealmRolesToUserAsync(user.Id, [role]);
await realm.Roles.RemoveRealmRolesFromUserAsync(user.Id, [role]);
```

## Client Roles

```csharp
var clientRole = await realm.Roles.CreateClientRoleAsync(
    client.Id,
    new CreateRoleRequest("orders-reader"));

await realm.Roles.AssignClientRolesToUserAsync(
    user.Id,
    client.Id,
    [clientRole]);
```

Client Roles também suportam listagem, consulta, atualização, exclusão e mappings para Groups.

## Groups

```csharp
var parent = await realm.Groups.CreateAsync(new CreateGroupRequest("engineering"));
var child = await realm.Groups.CreateAsync(
    new CreateGroupRequest("platform"),
    parent.Id);

await realm.Groups.AddUserAsync(child.Id, user.Id);
await realm.Roles.AssignRealmRolesToGroupAsync(child.Id, [role]);
```

## Protocol Mappers

```csharp
var mapper = await realm.ProtocolMappers.CreateClientScopeMapperAsync(
    scope.Id,
    new CreateProtocolMapperRequest("department", "oidc-usermodel-attribute-mapper")
    {
        Config = new Dictionary<string, string>
        {
            ["user.attribute"] = "department",
            ["claim.name"] = "department",
            ["jsonType.label"] = "String",
        },
    });
```

O mesmo serviço oferece CRUD de mappers associados diretamente a Clients.

## Identity Providers

```csharp
var provider = await realm.IdentityProviders.CreateAsync(
    new CreateIdentityProviderRequest("corporate-oidc", "oidc")
    {
        DisplayName = "Corporate login",
        Config = new Dictionary<string, string>
        {
            ["authorizationUrl"] = "https://idp.example.com/authorize",
            ["tokenUrl"] = "https://idp.example.com/token",
            ["clientId"] = "keycloak-broker",
        },
    });
```

Secrets presentes em `Config` não devem ser registrados em logs nem mantidos no código-fonte.

## Sessions

```csharp
var sessions = await realm.Sessions.GetUserSessionsAsync(user.Id);

foreach (var session in sessions)
{
    await realm.Sessions.RevokeAsync(session.Id, cancellationToken);
}
```

`LogoutAllAsync` encerra sessões do Realm inteiro e deve receber autorização e confirmação adequadas na aplicação consumidora.

## Events

```csharp
var events = await realm.Events.GetEventsAsync(new EventQuery
{
    Types = ["LOGIN_ERROR"],
    Max = 50,
});

var configuration = await realm.Events.GetConfigurationAsync();
await realm.Events.UpdateConfigurationAsync(configuration with
{
    EventsEnabled = true,
});
```

## Cancelamento

Todos os métodos assíncronos aceitam `CancellationToken`. Propague o token da requisição ou processo chamador para evitar trabalho abandonado.

## Tratamento de erros

```csharp
try
{
    await realm.Users.CreateAsync(request, cancellationToken);
}
catch (KeycloakConflictException)
{
    // Nome ou identificador já existe.
}
catch (KeycloakAuthorizationException)
{
    // A Service Account não possui a role administrativa necessária.
}
catch (KeycloakApiException exception)
{
    logger.LogError(exception, "Keycloak operation failed with {StatusCode}", exception.StatusCode);
}
```
