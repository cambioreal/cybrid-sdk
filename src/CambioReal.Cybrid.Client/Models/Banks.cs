namespace CambioReal.Cybrid.Models;

/// <summary>
/// Bank — resposta de <c>GET banks/{bank_guid}</c>. Shape da spec oficial v0.129, confirmado ao
/// vivo (2026-07-15, banco real "CambioReal Inc KYC").
/// </summary>
public sealed record CybridBank
{
    public string? Guid { get; init; }
    public string? OrganizationGuid { get; init; }
    public string? Name { get; init; }
    public string? Type { get; init; }
    public IReadOnlyList<string>? SupportedTradingSymbols { get; init; }

    /// <summary>
    /// Array de OBJETOS na resposta viva (diverge da spec v0.129, que declara array de string) —
    /// confirmado ao vivo 2026-07-15: <c>{symbol, country_code, participants_type, route}</c>.
    /// </summary>
    public IReadOnlyList<CybridPayoutSymbol>? SupportedPayoutSymbols { get; init; }
    public IReadOnlyList<string>? SupportedFiatAccountAssets { get; init; }
    public IReadOnlyList<string>? SupportedCountryCodes { get; init; }
    public IReadOnlyList<string>? Features { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>Símbolo de payout suportado — shape real da resposta viva (não documentado na spec).</summary>
public sealed record CybridPayoutSymbol
{
    public string? Symbol { get; init; }
    public string? CountryCode { get; init; }
    public string? ParticipantsType { get; init; }
    public string? Route { get; init; }
}
