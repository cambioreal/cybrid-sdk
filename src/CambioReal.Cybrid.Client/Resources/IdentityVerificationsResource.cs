using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Verificações de identidade (KYC/KYB/contraparte). <c>identity_verifications</c>.</summary>
public sealed class IdentityVerificationsResource
{
    private readonly CybridClient client;

    internal IdentityVerificationsResource(CybridClient client) => this.client = client;

    /// <summary>
    /// Lista verificações de identidade do bank. <c>GET identity_verifications</c> — gap fechado
    /// (só existia GET por guid + POST). Filtros opcionais confirmados na spec oficial
    /// (<c>guid</c>, <c>customer_guid</c>, <c>counterparty_guid</c>, <c>state</c>, <c>type</c>,
    /// <c>method</c> — todos comma-separated; <c>bank_guid</c> é sempre o configurado, não
    /// exposto). Ordenado por data de criação decrescente (spec).
    /// </summary>
    public Task<CybridListPage<CybridIdentityVerification>> ListAsync(
        int page = 0, int perPage = 20, string? guid = null, string? customerGuid = null,
        string? counterpartyGuid = null, string? state = null, string? type = null, string? method = null,
        CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridIdentityVerification>>(
            CybridPaths.List(CybridPaths.IdentityVerifications, client.BankGuid, page, perPage,
                CybridPaths.Filters(
                    ("guid", guid),
                    ("customer_guid", customerGuid),
                    ("counterparty_guid", counterpartyGuid),
                    ("state", state),
                    ("type", type),
                    ("method", method))),
            CybridScopes.IdentityVerificationsRead,
            cancellationToken);

    /// <summary>Consulta uma verificação. <c>GET identity_verifications/{guid}</c> — status por polling.</summary>
    public Task<CybridIdentityVerification> GetAsync(string guid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridIdentityVerification>(
            CybridPaths.IdentityVerification(guid), CybridScopes.IdentityVerificationsRead, cancellationToken);

    /// <summary>Inicia uma verificação. <c>POST identity_verifications</c>. Escrita não financeira.</summary>
    public Task<CybridIdentityVerification> CreateAsync(
        CreateCybridIdentityVerificationRequest request, CancellationToken cancellationToken = default) =>
        client.PostAsync<CreateCybridIdentityVerificationRequest, CybridIdentityVerification>(
            CybridPaths.IdentityVerifications, request, CybridScopes.IdentityVerificationsExecute, cancellationToken);
}
