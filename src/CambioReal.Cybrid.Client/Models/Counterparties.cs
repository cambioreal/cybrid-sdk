namespace CambioReal.Cybrid.Models;

/// <summary>Corpo de <c>POST counterparties</c> — spec oficial (<c>type</c>/<c>address</c> obrigatórios).</summary>
public sealed record CreateCybridCounterpartyRequest
{
    /// <summary><c>individual</c> ou <c>business</c>.</summary>
    public required string Type { get; init; }

    public required CybridAddress Address { get; init; }
    public string? CustomerGuid { get; init; }
    public CybridName? Name { get; init; }
    public string? DateOfBirth { get; init; }
    public string? EmailAddress { get; init; }
    public IReadOnlyList<CybridIdentificationNumber>? IdentificationNumbers { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
}

/// <summary>Counterparty — spec oficial v0.129, listagem validada ao vivo (11 reais).</summary>
public sealed record CybridCounterparty
{
    public string? Guid { get; init; }
    public string? Type { get; init; }
    public string? BankGuid { get; init; }
    public string? CustomerGuid { get; init; }

    /// <summary>Valores conhecidos: <c>storing</c>, <c>created</c>, <c>frozen</c> (conjunto aberto).</summary>
    public string? State { get; init; }

    public CybridName? Name { get; init; }
    public CybridAddress? Address { get; init; }
    public string? DateOfBirth { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
