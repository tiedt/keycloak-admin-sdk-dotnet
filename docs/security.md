# Segurança

## Credenciais

- Use Client Credentials e Service Account para aplicações servidoras.
- Armazene secrets fora de `appsettings.json` e do controle de versão.
- Rotacione Client Secrets e revogue credenciais não utilizadas.
- Use HTTPS fora de ambientes locais.
- Não reutilize credenciais administrativas entre ambientes.

## Privilégio mínimo

Conceda somente roles necessárias do `realm-management`. Separe aplicações de leitura e escrita quando possível. Não use o usuário bootstrap do Keycloak como credencial permanente de uma aplicação.

## Logs e telemetria

O SDK não registra:

- Access tokens ou refresh tokens.
- Client Secrets ou passwords.
- Corpos de requisição/resposta.
- Query strings, que podem conter username ou email.

Quando `IncludeRequestPath=true`, caminhos podem conter IDs de recursos e nomes de Realms. Desabilite essa opção se esses identificadores forem considerados sensíveis pela organização.

## IA e privacidade

Nunca envie automaticamente stack traces, prompts, dados pessoais ou configuração completa a um modelo externo sem política explícita. O contrato `KeycloakDiagnosticEvent` é sanitizado, mas o sistema consumidor ainda deve aplicar:

- Allowlist de campos.
- Redação adicional.
- Controle de retenção.
- Autorização para consulta dos incidentes.
- Restrição geográfica e contratual do provedor.
- Proteção contra prompt injection em dados externos.

## TLS

Não desabilite validação de certificados no SDK ou no `HttpClientHandler`. Em desenvolvimento, confie na CA local; em produção, utilize certificados válidos e rotação automatizada.

## Password Grant

O Password Grant permanece disponível apenas para compatibilidade e exige `AllowPasswordGrant=true`. Ele amplia a exposição de credenciais de usuário e não deve ser a escolha padrão.

## Threat model

Para controles, fronteiras de confiança e riscos residuais de OpenTelemetry e MCP, consulte [Threat model de observabilidade e MCP](threat-model-observability-mcp.md).
