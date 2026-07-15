namespace CambioReal.Cybrid.Models;

/// <summary>
/// Conta interna (fiat/trading) — spec oficial v0.129, listagem validada ao vivo (2026-07-15,
/// 65 contas reais). Valores monetários são inteiros na menor unidade do asset (padrão Cybrid).
/// </summary>
public sealed record CybridAccount
{
    public string? Guid { get; init; }

    /// <summary>Valores conhecidos: <c>trading</c>, <c>fiat</c>, <c>gas</c>, <c>fee</c> (conjunto aberto).</summary>
    public string? Type { get; init; }

    public string? Asset { get; init; }
    public string? Name { get; init; }
    public string? BankGuid { get; init; }
    public string? CustomerGuid { get; init; }
    public long? PlatformBalance { get; init; }
    public long? PlatformAvailable { get; init; }

    /// <summary>Valores conhecidos: <c>storing</c>, <c>created</c> (conjunto aberto).</summary>
    public string? State { get; init; }

    public IReadOnlyList<string>? Labels { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>Corpo de <c>POST accounts</c> — spec oficial.</summary>
public sealed record CreateCybridAccountRequest
{
    /// <summary><c>trading</c> ou <c>fiat</c>.</summary>
    public required string Type { get; init; }

    public required string Asset { get; init; }
    public required string Name { get; init; }
    public string? CustomerGuid { get; init; }
    public string? BankGuid { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
}
