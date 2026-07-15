using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Bank. <c>banks</c>.</summary>
public sealed class BanksResource
{
    private readonly CybridClient client;

    internal BanksResource(CybridClient client) => this.client = client;

    /// <summary>
    /// Consulta o bank configurado. <c>GET banks/{bank_guid}</c> — validado ao vivo (2026-07-15,
    /// banco real "CambioReal Inc KYC").
    /// </summary>
    public Task<CybridBank> GetAsync(CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridBank>(CybridPaths.Bank(client.BankGuid), CybridScopes.BanksRead, cancellationToken);
}
