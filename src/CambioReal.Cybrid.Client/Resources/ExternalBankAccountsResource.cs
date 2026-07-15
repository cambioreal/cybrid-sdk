using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Contas bancárias externas. <c>external_bank_accounts</c>.</summary>
public sealed class ExternalBankAccountsResource
{
    private readonly CybridClient client;

    internal ExternalBankAccountsResource(CybridClient client) => this.client = client;

    /// <summary>Lista contas externas do bank. <c>GET external_bank_accounts</c> — validado ao vivo (131 reais).</summary>
    public Task<CybridListPage<CybridExternalBankAccount>> ListAsync(
        int page = 0, int perPage = 20, string? customerGuid = null, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridExternalBankAccount>>(
            CybridPaths.List(CybridPaths.ExternalBankAccounts, client.BankGuid, page, perPage,
                customerGuid is null ? null : $"customer_guid={Uri.EscapeDataString(customerGuid)}"),
            CybridScopes.ExternalBankAccountsRead,
            cancellationToken);

    /// <summary>Consulta uma conta externa. <c>GET external_bank_accounts/{guid}</c>.</summary>
    public Task<CybridExternalBankAccount> GetAsync(string guid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridExternalBankAccount>(
            CybridPaths.ExternalBankAccount(guid), CybridScopes.ExternalBankAccountsRead, cancellationToken);

    /// <summary>
    /// Consulta uma conta externa com refresh de saldo — padrão do legado
    /// (<c>?force_balance_refresh=true&amp;include_balances=true&amp;include_pii=true</c>; exige o
    /// scope adicional <c>external_bank_accounts:pii:read</c>).
    /// </summary>
    public Task<CybridExternalBankAccount> GetWithBalancesAsync(string guid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridExternalBankAccount>(
            $"{CybridPaths.ExternalBankAccount(guid)}?force_balance_refresh=true&include_balances=true&include_pii=true",
            "external_bank_accounts:read external_bank_accounts:pii:read",
            cancellationToken);

    /// <summary>Registra uma conta externa. <c>POST external_bank_accounts</c>. Escrita não financeira; cleanup = <see cref="DeleteAsync"/>.</summary>
    public Task<CybridExternalBankAccount> CreateAsync(
        CreateCybridExternalBankAccountRequest request, CancellationToken cancellationToken = default) =>
        client.PostAsync<CreateCybridExternalBankAccountRequest, CybridExternalBankAccount>(
            CybridPaths.ExternalBankAccounts, request, CybridScopes.ExternalBankAccountsExecute, cancellationToken);

    /// <summary>Remove uma conta externa. <c>DELETE external_bank_accounts/{guid}</c> — devolve o recurso em estado de remoção.</summary>
    public Task<CybridExternalBankAccount> DeleteAsync(string guid, CancellationToken cancellationToken = default) =>
        client.DeleteAsync<CybridExternalBankAccount>(
            CybridPaths.ExternalBankAccount(guid), CybridScopes.ExternalBankAccountsExecute, cancellationToken);
}
