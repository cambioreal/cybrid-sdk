using System.Text.Json;

namespace CambioReal.Cybrid.Models;

/// <summary>Corpo de <c>POST identity_verifications</c> — spec oficial (<c>type</c> obrigatório).</summary>
public sealed record CreateCybridIdentityVerificationRequest
{
    /// <summary>Valores da spec: <c>kyc</c>, <c>bank_account</c>, <c>counterparty</c>.</summary>
    public required string Type { get; init; }

    /// <summary>
    /// Valores da spec: <c>watchlists</c>, <c>attested</c>, <c>document_submission</c>,
    /// <c>id_and_selfie</c>, etc. Combos confirmados no legado: <c>{bank_account,
    /// account_ownership}</c>, <c>{counterparty, watchlists}</c>, <c>{kyc, enhanced_due_diligence}</c>.
    /// </summary>
    public string? Method { get; init; }

    public string? CustomerGuid { get; init; }
    public string? CounterpartyGuid { get; init; }

    /// <summary>Verificação de titularidade de conta externa (<c>type: bank_account</c>) — confirmado no legado.</summary>
    public string? ExternalBankAccountGuid { get; init; }

    /// <summary>
    /// Atalhos de sandbox (<c>["passed_immediately"]</c>) — o legado só envia em dev; NUNCA usar
    /// em produção.
    /// </summary>
    public IReadOnlyList<string>? ExpectedBehaviours { get; init; }
    public string? CountryCode { get; init; }
    public CybridName? Name { get; init; }
    public CybridAddress? Address { get; init; }
    public string? DateOfBirth { get; init; }
    public IReadOnlyList<CybridIdentificationNumber>? IdentificationNumbers { get; init; }
    public string? PhoneNumber { get; init; }
    public string? EmailAddress { get; init; }
}

/// <summary>Identity verification — spec oficial v0.129.</summary>
public sealed record CybridIdentityVerification
{
    public string? Guid { get; init; }
    public string? Type { get; init; }
    public string? Method { get; init; }
    public string? CustomerGuid { get; init; }
    public string? CounterpartyGuid { get; init; }

    /// <summary>Valores conhecidos: <c>storing</c>, <c>waiting</c>, <c>expired</c>, <c>completed</c> (conjunto aberto).</summary>
    public string? State { get; init; }

    /// <summary>Valores conhecidos: <c>passed</c>, <c>failed</c> (conjunto aberto).</summary>
    public string? Outcome { get; init; }

    public IReadOnlyList<string>? FailureCodes { get; init; }
    public JsonElement? Options { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
