using Keycloak.AdminSdk.Common;
using Keycloak.AdminSdk.Features.ProtocolMappers.Models;

namespace Keycloak.AdminSdk.Abstractions.ProtocolMappers;

/// <summary>Manages protocol mappers attached to clients and client scopes.</summary>
public interface IKeycloakProtocolMapperService
{
    Task<IReadOnlyList<ProtocolMapperResource>> GetClientScopeMappersAsync(KeycloakResourceId scopeId, CancellationToken cancellationToken = default);
    Task<ProtocolMapperResource> CreateClientScopeMapperAsync(KeycloakResourceId scopeId, CreateProtocolMapperRequest request, CancellationToken cancellationToken = default);
    Task<ProtocolMapperResource> UpdateClientScopeMapperAsync(KeycloakResourceId scopeId, KeycloakResourceId mapperId, UpdateProtocolMapperRequest request, CancellationToken cancellationToken = default);
    Task DeleteClientScopeMapperAsync(KeycloakResourceId scopeId, KeycloakResourceId mapperId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProtocolMapperResource>> GetClientMappersAsync(KeycloakResourceId clientId, CancellationToken cancellationToken = default);
    Task<ProtocolMapperResource> CreateClientMapperAsync(KeycloakResourceId clientId, CreateProtocolMapperRequest request, CancellationToken cancellationToken = default);
    Task<ProtocolMapperResource> UpdateClientMapperAsync(KeycloakResourceId clientId, KeycloakResourceId mapperId, UpdateProtocolMapperRequest request, CancellationToken cancellationToken = default);
    Task DeleteClientMapperAsync(KeycloakResourceId clientId, KeycloakResourceId mapperId, CancellationToken cancellationToken = default);
}
