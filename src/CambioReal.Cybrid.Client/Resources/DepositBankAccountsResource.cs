using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Contas de depósito (virtual account com memo único). <c>deposit_bank_accounts</c>.</summary>
public sealed class DepositBankAccountsResource
{
    private readonly CybridClient client;

    internal DepositBankAccountsResource(CybridClient client) => this.client = client;

    /// <summary>
    /// Lista contas de depósito do bank. <c>GET deposit_bank_accounts</c>. Filtros opcionais
    /// confirmados na spec oficial (<c>guid</c>, <c>customer_guid</c>, <c>label</c>,
    /// <c>unique_memo_id</c>, <c>type</c>, <c>state</c>, <c>parent_deposit_bank_account_guid</c>;
    /// <c>bank_guid</c> é sempre o configurado, não exposto).
    /// </summary>
    public Task<CybridListPage<CybridDepositBankAccount>> ListAsync(
        int page = 0, int perPage = 20, string? guid = null, string? customerGuid = null, string? label = null,
        string? uniqueMemoId = null, string? type = null, string? state = null,
        string? parentDepositBankAccountGuid = null, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridDepositBankAccount>>(
            CybridPaths.List(CybridPaths.DepositBankAccounts, client.BankGuid, page, perPage,
                CybridPaths.Filters(
                    ("guid", guid),
                    ("customer_guid", customerGuid),
                    ("label", label),
                    ("unique_memo_id", uniqueMemoId),
                    ("type", type),
                    ("state", state),
                    ("parent_deposit_bank_account_guid", parentDepositBankAccountGuid))),
            CybridScopes.DepositBankAccountsRead,
            cancellationToken);

    /// <summary>Consulta uma conta de depósito. <c>GET deposit_bank_accounts/{guid}</c>.</summary>
    public Task<CybridDepositBankAccount> GetAsync(string guid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridDepositBankAccount>(
            CybridPaths.DepositBankAccount(guid), CybridScopes.DepositBankAccountsRead, cancellationToken);

    /// <summary>Cria uma conta de depósito. <c>POST deposit_bank_accounts</c>. Escrita não financeira.</summary>
    public Task<CybridDepositBankAccount> CreateAsync(
        CreateCybridDepositBankAccountRequest request, CancellationToken cancellationToken = default) =>
        client.PostAsync<CreateCybridDepositBankAccountRequest, CybridDepositBankAccount>(
            CybridPaths.DepositBankAccounts, request, CybridScopes.DepositBankAccountsExecute, cancellationToken);
}
