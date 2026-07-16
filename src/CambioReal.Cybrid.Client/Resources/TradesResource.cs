using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Trades (execução de cotações trading). <c>trades</c>.</summary>
public sealed class TradesResource
{
    private readonly CybridClient client;

    internal TradesResource(CybridClient client) => this.client = client;

    /// <summary>
    /// Lista trades do bank. <c>GET trades</c>. Filtros opcionais confirmados na spec oficial
    /// (<c>guid</c>, <c>customer_guid</c>, <c>account_guid</c>, <c>state</c>, <c>side</c>,
    /// <c>label</c>, faixas <c>created_at_gte/lt</c> e <c>updated_at_gte/lt</c> em ISO8601;
    /// <c>bank_guid</c> é sempre o configurado, não exposto).
    /// </summary>
    public Task<CybridListPage<CybridTrade>> ListAsync(
        int page = 0, int perPage = 20, string? guid = null, string? customerGuid = null,
        string? accountGuid = null, string? state = null, string? side = null, string? label = null,
        DateTimeOffset? createdAtGte = null, DateTimeOffset? createdAtLt = null,
        DateTimeOffset? updatedAtGte = null, DateTimeOffset? updatedAtLt = null,
        CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridTrade>>(
            CybridPaths.List(CybridPaths.Trades, client.BankGuid, page, perPage,
                CybridPaths.Filters(
                    ("guid", guid),
                    ("customer_guid", customerGuid),
                    ("account_guid", accountGuid),
                    ("state", state),
                    ("side", side),
                    ("label", label),
                    ("created_at_gte", CybridPaths.Iso(createdAtGte)),
                    ("created_at_lt", CybridPaths.Iso(createdAtLt)),
                    ("updated_at_gte", CybridPaths.Iso(updatedAtGte)),
                    ("updated_at_lt", CybridPaths.Iso(updatedAtLt)))),
            CybridScopes.TradesRead,
            cancellationToken);

    /// <summary>Consulta um trade. <c>GET trades/{guid}</c> — status por polling.</summary>
    public Task<CybridTrade> GetAsync(string tradeGuid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridTrade>(CybridPaths.Trade(tradeGuid), CybridScopes.TradesRead, cancellationToken);

    /// <summary>
    /// Executa uma cotação trading. <c>POST trades</c>. **Financial-write** — movimenta saldo
    /// entre contas fiat/trading; não executar contra sandbox sem autorização explícita
    /// (goal-loop §0.5).
    /// </summary>
    public Task<CybridTrade> CreateAsync(CreateCybridTradeRequest request, CancellationToken cancellationToken = default) =>
        client.PostAsync<CreateCybridTradeRequest, CybridTrade>(
            CybridPaths.Trades, request, CybridScopes.TradesExecute, cancellationToken);
}
