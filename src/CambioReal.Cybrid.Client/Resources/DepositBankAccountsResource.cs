using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Contas de depósito (virtual account com memo único). <c>deposit_bank_accounts</c>.</summary>
public sealed class DepositBankAccountsResource
{
    private readonly CybridClient client;

    internal DepositBankAccountsResource(CybridClient client) => this.client = client;

    /// <summary>Lista contas de depósito do bank. <c>GET deposit_bank_accounts</c>.</summary>
    public Task<CybridListPage<CybridDepositBankAccount>> ListAsync(
        int page = 0, int perPage = 20, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridDepositBankAccount>>(
            CybridPaths.List(CybridPaths.DepositBankAccounts, client.BankGuid, page, perPage),
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
