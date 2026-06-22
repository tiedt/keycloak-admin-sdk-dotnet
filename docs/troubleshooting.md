# Troubleshooting

## HTTP 400 — Bad Request

Possíveis causas:

- Campo obrigatório ausente.
- Enum ou formato incompatível com a versão do Keycloak.
- Atualização com representação incompleta.

Ação: valide o request tipado, confirme a versão do servidor e correlacione o TraceId com os logs do Keycloak.

## HTTP 401 — Unauthorized

Possíveis causas:

- Client Secret incorreto ou rotacionado.
- `AuthenticationRealm` incorreto.
- Client authentication ou Service Account desabilitada.
- Token expirado ou rejeitado por diferença de relógio.

Ação: teste o token endpoint, confira o relógio dos hosts e valide as credenciais no secret manager.

## HTTP 403 — Forbidden

Causa provável: token válido, mas sem role administrativa suficiente.

Ação: atribua a role mínima necessária do `realm-management` à Service Account. Evite resolver com `realm-admin` sem análise.

## HTTP 404 — Not Found

Possíveis causas:

- Contexto apontando para Realm errado.
- ID antigo após recriação do recurso.
- Nome de role ou Realm incorreto.

Ação: consulte novamente o recurso e use o ID retornado pelo SDK.

## HTTP 409 — Conflict

Causa provável: username, Client ID, Realm, Group, Role ou Client Scope já existe.

Ação: implemente fluxo idempotente com `Find*Async` antes da criação ou trate `KeycloakConflictException`.

## HTTP 429 — Too Many Requests

Ação: reduza concorrência, respeite `Retry-After` e revise limites do gateway. Operações de escrita não recebem retry automático para evitar duplicidade.

## HTTP 5xx

Possíveis causas:

- Exceção interna no Keycloak.
- Banco indisponível ou saturado.
- Representação incompatível com uma extensão do servidor.
- Falha no reverse proxy.

Ação: procure o TraceId nos logs do servidor, banco e proxy. O SDK não inclui o corpo da resposta em exceções para evitar vazamento.

## Timeout ou circuit breaker

Ação: confirme latência e disponibilidade antes de aumentar `AttemptTimeout` ou `TotalRequestTimeout`. Timeouts maiores podem apenas ocultar saturação.

## Interfaces setoriais não resolvem pela DI

Configure `DefaultRealm`. Sem ele, injete `IKeycloakAdminClient` e utilize `ForRealm(...)` explicitamente.

## Diagnóstico mínimo

Para suporte, colete:

- Timestamp UTC.
- TraceId.
- Status HTTP.
- Tipo da operação e método HTTP.
- Versão do pacote e do Keycloak.
- Logs do servidor no mesmo intervalo.

Não inclua tokens, secrets, passwords ou corpos com dados pessoais.
