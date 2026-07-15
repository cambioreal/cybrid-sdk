using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Preços spot. <c>prices</c>.</summary>
public sealed class PricesResource
{
    private readonly CybridClient client;

    internal PricesResource(CybridClient client) => this.client = client;

    /// <summary>
    /// Consulta preços. <c>GET prices?symbol=</c> — validado ao vivo (2026-07-15, USDC-USD).
    /// Resposta é um ARRAY na raiz (sem envelope de paginação).
    /// </summary>
    public Task<IReadOnlyList<CybridSymbolPrice>> ListAsync(
        string? symbol = null, CancellationToken cancellationToken = default) =>
        client.GetAsync<IReadOnlyList<CybridSymbolPrice>>(
            symbol is null ? CybridPaths.Prices : $"{CybridPaths.Prices}?symbol={Uri.EscapeDataString(symbol)}",
            CybridScopes.PricesRead,
            cancellationToken);
}
