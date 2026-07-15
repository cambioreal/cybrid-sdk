namespace CambioReal.Cybrid.Models;

/// <summary>Corpo de <c>POST customers</c> — spec oficial (<c>type</c> obrigatório).</summary>
public sealed record CreateCybridCustomerRequest
{
    /// <summary><c>individual</c> ou <c>business</c>.</summary>
    public required string Type { get; init; }

    public CybridName? Name { get; init; }
    public CybridAddress? Address { get; init; }
    public string? DateOfBirth { get; init; }
    public string? PhoneNumber { get; init; }
    public string? EmailAddress { get; init; }
    public IReadOnlyList<CybridIdentificationNumber>? IdentificationNumbers { get; init; }
    public string? Website { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
}

/// <summary>Customer — spec oficial v0.129, listagem validada ao vivo (208 customers reais).</summary>
public sealed record CybridCustomer
{
    public string? Guid { get; init; }
    public string? BankGuid { get; init; }
    public string? Type { get; init; }

    /// <summary>Valores conhecidos: <c>storing</c>, <c>unverified</c>, <c>verified</c>, <c>rejected</c>, <c>frozen</c> (conjunto aberto).</summary>
    public string? State { get; init; }

    public CybridName? Name { get; init; }
    public CybridAddress? Address { get; init; }
    public string? DateOfBirth { get; init; }
    public string? PhoneNumber { get; init; }
    public string? EmailAddress { get; init; }
    public IReadOnlyList<CybridIdentificationNumber>? IdentificationNumbers { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
