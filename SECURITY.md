# Política de segurança

## Versões

Durante a fase preview, somente a preview mais recente recebe correções de segurança. A política será ampliada antes da versão `1.0.0`.

## Reportar vulnerabilidade

Não abra issue pública com exploit, credencial ou dado real. Utilize um GitHub Security Advisory privado no repositório. Inclua versão do pacote e do Keycloak, impacto, pré-condições, reprodução mínima e mitigação conhecida.

Não inclua access tokens, Client Secrets, passwords, dumps de produção ou dados pessoais no relatório.

## Escopo

São relevantes, entre outros:

- vazamento de credenciais ou PII;
- bypass de autorização ou troca indevida de Realm;
- SSRF, desativação de TLS ou construção insegura de URLs;
- retry de mutações não idempotentes com efeitos duplicados;
- exposição ou mutação não autorizada por MCP;
- dependências vulneráveis com caminho de exploração aplicável.
