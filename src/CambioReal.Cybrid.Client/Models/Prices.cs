namespace CambioReal.Cybrid.Models;

/// <summary>
/// Preço spot de um símbolo — <c>GET prices</c>, validado ao vivo (2026-07-15). Preços em
/// inteiros na menor unidade da moeda de cotação (padrão Cybrid).
/// </summary>
public sealed record CybridSymbolPrice
{
    public string? Symbol { get; init; }
    public string? Type { get; init; }
    public long? BuyPrice { get; init; }
    public long? SellPrice { get; init; }
    public DateTimeOffset? BuyPriceLastUpdatedAt { get; init; }
    public DateTimeOffset? SellPriceLastUpdatedAt { get; init; }
    public string? CountryCode { get; init; }
    public string? ParticipantsType { get; init; }
    public string? Route { get; init; }
}
