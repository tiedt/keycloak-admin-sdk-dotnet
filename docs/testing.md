# Estratégia e matriz de testes

## Camadas

| Suíte | Objetivo |
|---|---|
| UnitTests | regras de configuração, autenticação, contexto, provisionamento e observabilidade |
| ArchitectureTests | impedir implementações públicas e preservar separação de contratos |
| Diagnostics.Mcp.UnitTests | buffer, filtros, resumo e limites do adapter opcional |
| IntegrationTests | executar o ciclo administrativo real na Admin REST API |

## Matriz Keycloak

O workflow `keycloak-matrix.yml` executa a suíte de integração nas imagens oficiais:

- 24.0.5
- 25.0.6
- 26.0.8
- 26.6.3

A matriz roda em pull requests, pushes para `main`, acionamento manual e semanalmente. `fail-fast` permanece desabilitado para mostrar todas as incompatibilidades em uma única execução.

## Teste local

```powershell
docker-compose up -d
dotnet test tests/Keycloak.AdminSdk.IntegrationTests --configuration Release --settings tests/Keycloak.AdminSdk.IntegrationTests/local.runsettings
```

O teste cria recursos isolados com nomes aleatórios e os remove ao final. Use somente ambiente descartável; nunca aponte a suíte para produção.
