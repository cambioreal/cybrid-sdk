using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Contas internas (fiat/trading). <c>accounts</c>.</summary>
public sealed class AccountsResource
{
    private readonly CybridClient client;

    internal AccountsResource(CybridClient client) => this.client = client;

    /// <summary>Lista contas do bank. <c>GET accounts</c> — validado ao vivo (65 contas reais).</summary>
    public Task<CybridListPage<CybridAccount>> ListAsync(
        int page = 0, int perPage = 20, string? customerGuid = null, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridAccount>>(
            CybridPaths.List(CybridPaths.Accounts, client.BankGuid, page, perPage,
                customerGuid is null ? null : $"customer_guid={Uri.EscapeDataString(customerGuid)}"),
            CybridScopes.AccountsRead,
            cancellationToken);

    /// <summary>Consulta uma conta. <c>GET accounts/{guid}</c>.</summary>
    public Task<CybridAccount> GetAsync(string accountGuid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridAccount>(CybridPaths.Account(accountGuid), CybridScopes.AccountsRead, cancellationToken);

    /// <summary>Cria uma conta (fiat/trading). <c>POST accounts</c>. Escrita não financeira.</summary>
    public Task<CybridAccount> CreateAsync(CreateCybridAccountRequest request, CancellationToken cancellationToken = default) =>
        client.PostAsync<CreateCybridAccountRequest, CybridAccount>(
            CybridPaths.Accounts, request, CybridScopes.AccountsExecute, cancellationToken);
}
