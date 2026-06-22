# Política de compatibilidade

## Plataformas suportadas

| Componente | Suporte atual |
|---|---|
| Runtime | .NET 10 |
| Keycloak | 24.0.5, 25.0.6, 26.0.8 e 26.6.3 na matriz automatizada |
| Transporte | HTTPS em produção; HTTP somente para desenvolvimento local |
| Autenticação administrativa | Client Credentials; Password Grant legado e opt-in |

Uma versão é considerada suportada quando a suíte de integração passa contra a imagem oficial correspondente. Versões intermediárias da mesma linha podem funcionar, mas só as versões presentes na matriz são garantidas.

## Versionamento do pacote

- Releases `0.x` são previews: mudanças incompatíveis podem ocorrer em uma versão minor e serão registradas no changelog.
- Depois de `1.0.0`, a API pública seguirá Semantic Versioning: breaking changes somente em major releases.
- Correções de segurança podem remover ou restringir comportamento inseguro independentemente da cadência normal.
- Tipos e membros públicos são controlados pelos arquivos `PublicAPI.Shipped.txt`; alterações acidentais falham no build.

## Janela de suporte

- O projeto mantém a última versão estável de cada linha de Keycloak explicitamente listada na matriz.
- Quando uma linha deixa de receber correções upstream, sua remoção será anunciada com pelo menos uma release minor de antecedência.
- O suporte a novas versões começa como experimental, passa pela suíte completa e só então entra nesta tabela.

## Política de depreciação

APIs serão marcadas com `ObsoleteAttribute` e terão alternativa documentada antes da remoção, exceto em vulnerabilidades críticas. Durante preview, a equipe pode ajustar nomes e contratos com nota explícita no changelog.
