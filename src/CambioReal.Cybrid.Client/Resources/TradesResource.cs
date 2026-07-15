using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Trades (execução de cotações trading). <c>trades</c>.</summary>
public sealed class TradesResource
{
    private readonly CybridClient client;

    internal TradesResource(CybridClient client) => this.client = client;

    /// <summary>Lista trades do bank. <c>GET trades</c>.</summary>
    public Task<CybridListPage<CybridTrade>> ListAsync(
        int page = 0, int perPage = 20, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridTrade>>(
            CybridPaths.List(CybridPaths.Trades, client.BankGuid, page, perPage),
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
