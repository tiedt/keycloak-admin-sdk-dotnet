# Publicação da preview

## Pré-requisitos

- Repositório hospedado no GitHub.
- Secret `NUGET_API_KEY` configurado no ambiente do repositório.
- `OWNER` e URLs do projeto ajustados nos metadados e no changelog.
- CI e matriz de compatibilidade verdes.

## Publicar

1. Atualize `CHANGELOG.md` e confirme a versão nos dois projetos empacotáveis.
2. Execute localmente `dotnet format`, build, testes e pack.
3. Crie e envie uma tag prerelease:

```powershell
git tag v0.1.0-preview.1
git push origin v0.1.0-preview.1
```

O workflow de release recompila, testa, gera os dois pacotes, cria SBOM CycloneDX, publica no NuGet.org e cria uma GitHub prerelease.

## Critérios para sair de preview

- API pública validada por pelo menos dois consumidores reais.
- Matriz de Keycloak estável por releases consecutivas.
- Cobertura dos módulos centrais sem mudanças incompatíveis pendentes.
- Threat model revisado e processo de reporte de vulnerabilidade ativo.
- Documentação e migração testadas a partir de um projeto vazio.
