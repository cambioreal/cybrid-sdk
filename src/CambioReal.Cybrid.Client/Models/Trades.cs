namespace CambioReal.Cybrid.Models;

/// <summary>Corpo de <c>POST trades</c> — spec oficial (<c>quote_guid</c> obrigatório).</summary>
public sealed record CreateCybridTradeRequest
{
    public required string QuoteGuid { get; init; }
    public string? TradeType { get; init; }
    public string? FiatAccountGuid { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
}

/// <summary>Trade — execução de uma cotação trading. Spec oficial v0.129.</summary>
public sealed record CybridTrade
{
    public string? Guid { get; init; }
    public string? TradeType { get; init; }
    public string? CustomerGuid { get; init; }
    public string? QuoteGuid { get; init; }
    public string? Symbol { get; init; }
    public string? Side { get; init; }

    /// <summary>Valores conhecidos: <c>storing</c>, <c>pending</c>, <c>cancelled</c>, <c>completed</c>, <c>settling</c>, <c>failed</c> (conjunto aberto).</summary>
    public string? State { get; init; }

    public string? FailureCode { get; init; }
    public long? ReceiveAmount { get; init; }
    public long? DeliverAmount { get; init; }
    public long? Fee { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
