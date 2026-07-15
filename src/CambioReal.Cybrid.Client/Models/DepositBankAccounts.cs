using System.Text.Json;

namespace CambioReal.Cybrid.Models;

/// <summary>Corpo de <c>POST deposit_bank_accounts</c> — subconjunto da spec oficial.</summary>
public sealed record CreateCybridDepositBankAccountRequest
{
    public string? CustomerGuid { get; init; }
    public string? AccountGuid { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
}

/// <summary>Deposit bank account — spec oficial v0.129.</summary>
public sealed record CybridDepositBankAccount
{
    public string? Guid { get; init; }
    public string? Type { get; init; }
    public string? BankGuid { get; init; }
    public string? CustomerGuid { get; init; }
    public string? AccountGuid { get; init; }
    public string? Asset { get; init; }

    /// <summary>Valores conhecidos: <c>storing</c>, <c>created</c> (conjunto aberto).</summary>
    public string? State { get; init; }

    public string? UniqueMemoId { get; init; }
    public string? CounterpartyName { get; init; }
    public CybridAddress? CounterpartyAddress { get; init; }
    public JsonElement? AccountDetails { get; init; }
    public JsonElement? RoutingDetails { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
