# Threat model: observabilidade, OpenTelemetry e MCP

## Escopo e ativos

Ativos protegidos: credenciais administrativas, tokens, PII de usuários, configuração de Realms, trilhas de auditoria, disponibilidade da aplicação e integridade das recomendações produzidas por IA.

Fronteiras de confiança:

```text
Aplicação -> SDK -> Keycloak
     |-> OpenTelemetry -> Collector -> backend de observabilidade
     `-> buffer sanitizado -> endpoint MCP -> agente de IA
```

OpenTelemetry é o canal de exportação. MCP é apenas uma interface opcional de consulta diagnóstica read-only; ele não substitui Collector, Loki, Tempo ou outro backend.

## Ameaças e controles

| Ameaça | Impacto | Controles implementados/recomendados | Risco residual |
|---|---|---|---|
| Vazamento de token, secret ou senha em logs | comprometimento administrativo | SDK não registra headers, payloads, tokens, secrets, passwords ou query strings; allowlist de campos | código consumidor ainda pode registrar dados sensíveis |
| PII em paths ou IDs | privacidade e correlação indevida | `IncludeRequestPath` é opt-in/configurável; nunca inclui query string | nomes de Realm e IDs podem ser sensíveis |
| Acesso não autorizado ao MCP | exposição de incidentes | autenticação forte, scope dedicado, TLS, segmentação de rede e rate limit no host | credencial MCP comprometida permite leitura do buffer |
| Prompt injection via telemetria | recomendação enganosa | dados tratados como não confiáveis; tools read-only; sem execução automática; saída com evidências | IA pode inferir causa incorreta |
| Exfiltração por exporter OTLP | perda de dados | exporter é opt-in e configurado pela aplicação; TLS/mTLS, egress allowlist e retenção no Collector | backend externo continua sendo uma nova fronteira |
| Negação de serviço por cardinalidade ou volume | custo/memória/latência | métricas sem Realm/path/IDs; buffer limitado; paginação e limite de consulta MCP | bursts ainda podem pressionar backend |
| Alteração ou exclusão de evidência | investigação incorreta | backend central imutável, RBAC, retenção e auditoria; MCP somente leitura | buffer em memória não é evidência durável |
| Confused deputy entre Realms | alteração no tenant errado | contexto de Realm imutável e explícito; credencial com privilégio mínimo | permissões amplas do Client ainda atravessam Realms |
| SSRF/configuração maliciosa do endpoint | acesso a rede interna | ServerUrl controlada na inicialização, configuração protegida e validação TLS | operador com acesso à configuração pode redirecionar tráfego |

## Requisitos para produção

- Não exponha MCP publicamente; prefira rede privada e identidade de workload.
- Separe scopes `keycloak.admin` e `keycloak.diagnostics`.
- Aplique mTLS ou TLS validado entre aplicação, Collector, backend e gateway MCP.
- Configure retenção, residência de dados, auditoria e redação no Collector.
- Nunca permita que uma recomendação da IA execute mutações automaticamente.
- Use TraceId como ponte; não envie payload completo para o modelo.
- Teste periodicamente que tokens e secrets não aparecem nos exports.

## Decisão de posicionamento

O produto principal é o SDK administrativo confiável. O adapter MCP permanece em pacote separado, desabilitado por padrão e documentado como extensão avançada. A adoção do core não depende de IA nem de MCP.
