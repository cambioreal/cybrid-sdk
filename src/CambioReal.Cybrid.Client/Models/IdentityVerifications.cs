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

    /// <summary>
    /// Exige coleta do tax id (SSN nos EUA, SIN no Canadá) durante a verificação. Opcional quando
    /// <c>type: kyc, method: id_and_selfie</c>.
    /// </summary>
    public bool? RequireTaxId { get; init; }

    /// <summary>Aliases do customer. Opcional quando <c>method</c> é <c>attested_business_registration</c> ou <c>watchlists</c>.</summary>
    public IReadOnlyList<CybridAlias>? Aliases { get; init; }

    /// <summary>Website do customer — obrigatório em business registration V2, opcional em V3.</summary>
    public string? Website { get; init; }

    /// <summary>Natureza do negócio — obrigatório quando <c>method: attested_business_registration</c>.</summary>
    public string? NatureOfBusiness { get; init; }

    /// <summary>Guids dos customers diretores do negócio — obrigatório quando <c>method: attested_business_registration</c>.</summary>
    public IReadOnlyList<string>? DirectorCustomerGuids { get; init; }

    /// <summary>Ultimate beneficial owners (≥25% de participação) — obrigatório em V2, opcional em V3.</summary>
    public IReadOnlyList<CybridUltimateBeneficialOwner>? UltimateBeneficialOwners { get; init; }

    /// <summary>Guids de arquivos de suporte à verificação — obrigatório para os métodos <c>attested_business_registration</c>/<c>attested_business_associate</c>/<c>attested_id_and_database</c>.</summary>
    public IReadOnlyList<string>? SupportingFileGuids { get; init; }

    /// <summary>Endereço registrado do negócio — obrigatório em business registration V3.</summary>
    public CybridAddress? RegisteredAddress { get; init; }

    /// <summary>Indústria em que o negócio opera — obrigatório em business registration V3.</summary>
    public string? BusinessIndustry { get; init; }

    /// <summary>Origem dos fundos do negócio — obrigatório em business registration V3.</summary>
    public string? BusinessFundsSource { get; init; }

    /// <summary>Destino dos fundos do negócio — obrigatório em business registration V3.</summary>
    public string? BusinessFundsDestination { get; init; }

    /// <summary>Ocupação do customer — obrigatório quando <c>method: attested_business_associate</c>.</summary>
    public string? Occupation { get; init; }

    /// <summary>Se a biometria foi verificada — obrigatório quando <c>method</c> é <c>attested_business_associate</c>/<c>attested_id_and_database</c>.</summary>
    public bool? BiometricsVerified { get; init; }
}

/// <summary>Alias (nome alternativo) de customer/counterparty — spec oficial (<c>{full}</c>).</summary>
public sealed record CybridAlias
{
    public string? Full { get; init; }
}

/// <summary>Ultimate beneficial owner de um negócio — spec oficial (<c>PostUltimateBeneficialOwner</c>).</summary>
public sealed record CybridUltimateBeneficialOwner
{
    public required string CustomerGuid { get; init; }
    public required decimal OwnershipPercentage { get; init; }
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
