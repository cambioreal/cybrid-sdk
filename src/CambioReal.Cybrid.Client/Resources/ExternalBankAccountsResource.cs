using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Contas bancárias externas. <c>external_bank_accounts</c>.</summary>
public sealed class ExternalBankAccountsResource
{
    private readonly CybridClient client;

    internal ExternalBankAccountsResource(CybridClient client) => this.client = client;

    /// <summary>
    /// Lista contas externas do bank. <c>GET external_bank_accounts</c> — validado ao vivo (131
    /// reais). Filtros opcionais confirmados na spec oficial (<c>guid</c>, <c>customer_guid</c>,
    /// <c>counterparty_guid</c>, <c>asset</c>, <c>state</c>; <c>bank_guid</c> é sempre o
    /// configurado, não exposto).
    /// </summary>
    public Task<CybridListPage<CybridExternalBankAccount>> ListAsync(
        int page = 0, int perPage = 20, string? customerGuid = null, string? guid = null,
        string? counterpartyGuid = null, string? asset = null, string? state = null,
        CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridExternalBankAccount>>(
            CybridPaths.List(CybridPaths.ExternalBankAccounts, client.BankGuid, page, perPage,
                CybridPaths.Filters(
                    ("customer_guid", customerGuid),
                    ("guid", guid),
                    ("counterparty_guid", counterpartyGuid),
                    ("asset", asset),
                    ("state", state))),
            CybridScopes.ExternalBankAccountsRead,
            cancellationToken);

    /// <summary>Consulta uma conta externa. <c>GET external_bank_accounts/{guid}</c>.</summary>
    public Task<CybridExternalBankAccount> GetAsync(string guid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridExternalBankAccount>(
            CybridPaths.ExternalBankAccount(guid), CybridScopes.ExternalBankAccountsRead, cancellationToken);

    /// <summary>
    /// Consulta uma conta externa com controle explícito de <c>force_balance_refresh</c>/
    /// <c>include_balances</c>/<c>include_pii</c> — os três parâmetros de query documentados na
    /// spec oficial que <see cref="GetAsync(string,CancellationToken)"/> não expõe.
    /// <paramref name="includePii"/> soma o scope <see cref="CybridScopes.ExternalBankAccountsPiiRead"/>
    /// ao token emitido (exigido pela API quando <c>include_pii=true</c>).
    /// </summary>
    public Task<CybridExternalBankAccount> GetAsync(
        string guid, bool forceBalanceRefresh, bool includeBalances, bool includePii,
        CancellationToken cancellationToken = default)
    {
        var query = CybridPaths.Filters(
            ("force_balance_refresh", CybridPaths.Bool(forceBalanceRefresh)),
            ("include_balances", CybridPaths.Bool(includeBalances)),
            ("include_pii", CybridPaths.Bool(includePii)));

        var path = query is null ? CybridPaths.ExternalBankAccount(guid) : $"{CybridPaths.ExternalBankAccount(guid)}?{query}";
        var scope = includePii
            ? $"{CybridScopes.ExternalBankAccountsRead} {CybridScopes.ExternalBankAccountsPiiRead}"
            : CybridScopes.ExternalBankAccountsRead;

        return client.GetAsync<CybridExternalBankAccount>(path, scope, cancellationToken);
    }

    /// <summary>
    /// Consulta uma conta externa com refresh de saldo — padrão do legado (força refresh + inclui
    /// saldos e PII). Atalho para
    /// <see cref="GetAsync(string,bool,bool,bool,CancellationToken)"/> com os três parâmetros em
    /// <see langword="true"/>.
    /// </summary>
    public Task<CybridExternalBankAccount> GetWithBalancesAsync(string guid, CancellationToken cancellationToken = default) =>
        GetAsync(guid, forceBalanceRefresh: true, includeBalances: true, includePii: true, cancellationToken);

    /// <summary>Registra uma conta externa. <c>POST external_bank_accounts</c>. Escrita não financeira; cleanup = <see cref="DeleteAsync"/>.</summary>
    public Task<CybridExternalBankAccount> CreateAsync(
        CreateCybridExternalBankAccountRequest request, CancellationToken cancellationToken = default) =>
        client.PostAsync<CreateCybridExternalBankAccountRequest, CybridExternalBankAccount>(
            CybridPaths.ExternalBankAccounts, request, CybridScopes.ExternalBankAccountsExecute, cancellationToken);

    /// <summary>Remove uma conta externa. <c>DELETE external_bank_accounts/{guid}</c> — devolve o recurso em estado de remoção.</summary>
    public Task<CybridExternalBankAccount> DeleteAsync(string guid, CancellationToken cancellationToken = default) =>
        client.DeleteAsync<CybridExternalBankAccount>(
            CybridPaths.ExternalBankAccount(guid), CybridScopes.ExternalBankAccountsExecute, cancellationToken);

    /// <summary>
    /// Atualiza uma conta externa. <c>PATCH external_bank_accounts/{guid}</c>. Único uso
    /// documentado pela spec: forçar <c>state</c> para <c>completed</c> (marca como verificada) ou
    /// <c>refresh_required</c>. Escrita não financeira; scope distinto de
    /// <see cref="CybridScopes.ExternalBankAccountsExecute"/>.
    /// </summary>
    public Task<CybridExternalBankAccount> PatchAsync(
        string guid, PatchCybridExternalBankAccountRequest request, CancellationToken cancellationToken = default) =>
        client.PatchAsync<PatchCybridExternalBankAccountRequest, CybridExternalBankAccount>(
            CybridPaths.ExternalBankAccount(guid), request, CybridScopes.ExternalBankAccountsWrite, cancellationToken);
}
