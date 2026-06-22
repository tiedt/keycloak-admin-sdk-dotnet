# Cobertura da Admin REST API

Este documento descreve a cobertura funcional da release `0.1.0-preview.1`. “Suportado” significa contrato público fortemente tipado e teste automatizado; não significa cobertura integral de todos os parâmetros oferecidos pelo Keycloak.

## Suportado

| Área | Operações |
|---|---|
| Realms | listar, consultar, criar, atualizar e excluir |
| Users | listar, consultar, criar, atualizar, excluir e redefinir senha |
| Clients | CRUD, localizar por `clientId`, associar scopes default/optional e provisionamento fluído |
| Client Scopes | CRUD |
| Roles | Realm Roles e Client Roles; mappings para Users e Groups |
| Groups | CRUD hierárquico e associação/remoção de Users |
| Protocol Mappers | CRUD em Clients e Client Scopes |
| Identity Providers | CRUD de instâncias configuradas |
| Sessions | listar por User/Client, revogar sessão, logout global e push de revogação |
| Events | consultar/limpar eventos e admin events; consultar/atualizar configuração |

## Ainda não suportado

- Organizations e seus memberships.
- User Federation e configuração de component providers (LDAP/Kerberos).
- Authentication flows, executions e required actions customizadas.
- Identity Provider mappers, importação de configuração e endpoints de descoberta.
- Client initial access, registration access tokens e client policies/profiles.
- Client scopes dedicados e avaliação de protocol mappers/scopes.
- Fine-grained admin permissions e permission tickets.
- Consents, offline sessions e credenciais detalhadas de usuários.
- Impersonation, execute-actions-email, verify-email e envio de reset por e-mail.
- Attack detection e limpeza de falhas de login.
- Keys, certificates, secret rotation e geração/registro de chaves de Clients.
- Partial import/export, importação completa de Realm e exportação de usuários.
- Localization texts, themes e configuração SMTP fortemente tipada.
- Componentes genéricos, subgroups paginados e contagens administrativas específicas.

Endpoints não listados não devem ser considerados suportados. Solicitações de cobertura devem incluir versão do Keycloak, endpoint, caso de uso e permissões mínimas necessárias.
