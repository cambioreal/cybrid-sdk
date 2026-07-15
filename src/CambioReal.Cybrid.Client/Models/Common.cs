namespace CambioReal.Cybrid.Models;

/// <summary>
/// Envelope de paginação das listagens da Cybrid — validado ao vivo (2026-07-15):
/// <c>{"total":93,"page":0,"per_page":1,"objects":[...]}</c>.
/// </summary>
public sealed record CybridListPage<T>
{
    public long Total { get; init; }
    public long Page { get; init; }
    public long PerPage { get; init; }
    public IReadOnlyList<T> Objects { get; init; } = [];
}

/// <summary>Nome estruturado (customers/counterparties) — <c>{first, middle, last, full}</c> na spec oficial.</summary>
public sealed record CybridName
{
    public string? First { get; init; }
    public string? Middle { get; init; }
    public string? Last { get; init; }
    public string? Full { get; init; }
}

/// <summary>Endereço estruturado — spec oficial (<c>street/street2/city/subdivision/postal_code/country_code</c>).</summary>
public sealed record CybridAddress
{
    public string? Street { get; init; }
    public string? Street2 { get; init; }
    public string? City { get; init; }
    public string? Subdivision { get; init; }
    public string? PostalCode { get; init; }
    public string? CountryCode { get; init; }
}

/// <summary>Documento de identificação — spec oficial (<c>type/issuing_country_code/identification_number</c>).</summary>
public sealed record CybridIdentificationNumber
{
    public string? Type { get; init; }
    public string? IssuingCountryCode { get; init; }
    public string? IdentificationNumber { get; init; }
}
