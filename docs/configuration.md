# Configuração completa

## Opções principais

| Opção | Obrigatória | Padrão | Finalidade |
|---|---:|---|---|
| `ServerUrl` | Sim | — | URL base do Keycloak. |
| `DefaultRealm` | Não | `null` | Realm usado pelas interfaces setoriais resolvidas diretamente por DI. |
| `AuthenticationRealm` | Sim | `master` | Realm que emite o token administrativo. |
| `Authentication` | Sim | Client Credentials | Credenciais e renovação de token. |
| `Resilience` | Não | Valores seguros | Retry e timeouts. |
| `Observability` | Não | Habilitada, sem logs de sucesso | Logs, traces, métricas e diagnósticos. |

## Client Credentials — recomendado

```json
"Authentication": {
  "Flow": "ClientCredentials",
  "ClientCredentials": {
    "ClientId": "admin-service",
    "ClientSecret": "${SECRET}",
    "Scopes": []
  }
}
```

O SDK mantém um token em memória e sincroniza a renovação para impedir várias solicitações simultâneas ao token endpoint.

## Password Grant — compatibilidade

```json
"Authentication": {
  "Flow": "Password",
  "Password": {
    "ClientId": "admin-cli",
    "Username": "admin",
    "Password": "${PASSWORD}",
    "AllowPasswordGrant": true
  }
}
```

Esse fluxo exige opt-in explícito. Prefira Client Credentials em serviços e automações.

## Resiliência

```json
"Resilience": {
  "MaxRetryAttempts": 3,
  "RetryBaseDelay": "00:00:00.500",
  "AttemptTimeout": "00:00:10",
  "TotalRequestTimeout": "00:00:30"
}
```

O pipeline utiliza retry exponencial com jitter, circuit breaker e timeouts. Operações administrativas não idempotentes, como `POST`, não recebem retry automático. A obtenção de token pode ser repetida porque é segura para essa finalidade.

## Observabilidade

```json
"Observability": {
  "Enabled": true,
  "LogSuccessfulRequests": false,
  "EmitDiagnosticHints": true,
  "IncludeRequestPath": true,
  "PublishDiagnosticEvents": true
}
```

Defina `Enabled=false` para desabilitar toda instrumentação específica do SDK. O `HttpClient` do .NET ainda pode ser instrumentado separadamente pela aplicação.

## Validação no startup

As opções são validadas com `ValidateOnStart`. URL inválida, credenciais ausentes, Password Grant sem opt-in e timeouts incoerentes impedem a inicialização da aplicação com erro explícito.
