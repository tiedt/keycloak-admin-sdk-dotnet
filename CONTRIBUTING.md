# Contribuindo

## Ambiente

- .NET SDK 10 conforme `global.json`.
- Docker para os testes reais contra Keycloak.

## Validação obrigatória

```powershell
dotnet restore Keycloak.AdminSdk.sln
dotnet format Keycloak.AdminSdk.sln --verify-no-changes --no-restore
dotnet build Keycloak.AdminSdk.sln --configuration Release --no-restore --no-incremental
dotnet test Keycloak.AdminSdk.sln --configuration Release --no-build
dotnet list Keycloak.AdminSdk.sln package --vulnerable --include-transitive
```

Para a integração local:

```powershell
docker-compose up -d
dotnet test tests/Keycloak.AdminSdk.IntegrationTests --configuration Release --settings tests/Keycloak.AdminSdk.IntegrationTests/local.runsettings
```

## Regras arquiteturais

- Cada área administrativa possui uma interface segregada.
- Contextos de Realm são imutáveis; não use estado global para selecionar tenant.
- Implementações, DTOs remotos e detalhes HTTP permanecem internos.
- Novas operações aceitam `CancellationToken` e usam modelos fortemente tipados.
- Não registre tokens, secrets, passwords, payloads ou query strings.
- Atualizações da API pública exigem baseline, teste, documentação e changelog.

Pull requests devem informar versões do Keycloak testadas, permissões administrativas necessárias e eventual impacto incompatível.
