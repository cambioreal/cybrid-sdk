using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Verificações de identidade (KYC/KYB/contraparte). <c>identity_verifications</c>.</summary>
public sealed class IdentityVerificationsResource
{
    private readonly CybridClient client;

    internal IdentityVerificationsResource(CybridClient client) => this.client = client;

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
