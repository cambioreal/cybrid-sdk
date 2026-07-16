using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Cotações. <c>quotes</c>.</summary>
public sealed class QuotesResource
{
    private readonly CybridClient client;

    internal QuotesResource(CybridClient client) => this.client = client;

    /// <summary>
    /// Lista cotações do bank. <c>GET quotes</c> — validado ao vivo (94 reais). Filtros opcionais
    /// confirmados na spec oficial (<c>guid</c>, <c>product_type</c>, <c>customer_guid</c>,
    /// <c>side</c>; <c>bank_guid</c> é sempre o configurado, não exposto).
    /// </summary>
    public Task<CybridListPage<CybridQuote>> ListAsync(
        int page = 0, int perPage = 20, string? guid = null, string? productType = null,
        string? customerGuid = null, string? side = null, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridQuote>>(
            CybridPaths.List(CybridPaths.Quotes, client.BankGuid, page, perPage,
                CybridPaths.Filters(
                    ("guid", guid),
                    ("product_type", productType),
                    ("customer_guid", customerGuid),
                    ("side", side))),
            CybridScopes.QuotesRead,
            cancellationToken);

    /// <summary>Consulta uma cotação. <c>GET quotes/{guid}</c>.</summary>
    public Task<CybridQuote> GetAsync(string quoteGuid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridQuote>(CybridPaths.Quote(quoteGuid), CybridScopes.QuotesRead, cancellationToken);

    /// <summary>
    /// Cria uma cotação. <c>POST quotes</c>. A cotação em si não move fundos e expira sozinha
    /// (<c>expires_at</c>) — é a EXECUÇÃO dela (trade/transfer) que é financeira.
    /// </summary>
    public Task<CybridQuote> CreateAsync(CreateCybridQuoteRequest request, CancellationToken cancellationToken = default) =>
        client.PostAsync<CreateCybridQuoteRequest, CybridQuote>(
            CybridPaths.Quotes, request, CybridScopes.QuotesExecute, cancellationToken);
}
