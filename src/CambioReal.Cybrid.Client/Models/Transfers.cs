using System.Text.Json;

namespace CambioReal.Cybrid.Models;

/// <summary>
/// Corpo de <c>POST transfers</c> — subconjunto da spec oficial v0.129 usado pelos fluxos da
/// plataforma (<c>quote_guid</c>/<c>transfer_type</c> obrigatórios).
/// </summary>
public sealed record CreateCybridTransferRequest
{
    public required string QuoteGuid { get; init; }

    /// <summary>Valores da spec: <c>funding</c>, <c>crypto</c>, <c>instant_funding</c>, <c>inter_account</c>, <c>book</c>.</summary>
    public required string TransferType { get; init; }

    public string? ExternalBankAccountGuid { get; init; }
    public string? FiatAccountGuid { get; init; }
    public string? SourceAccountGuid { get; init; }
    public string? DestinationAccountGuid { get; init; }
    public string? CustomerGuid { get; init; }
    public string? PaymentRail { get; init; }
    public string? BeneficiaryMemo { get; init; }
    public string? ExternalWalletGuid { get; init; }
    public IReadOnlyList<CybridTransferParticipant>? SourceParticipants { get; init; }
    public IReadOnlyList<CybridTransferParticipant>? DestinationParticipants { get; init; }

    /// <summary>Identificador da deposit bank account — só válido para withdrawals. Opcional quando <c>transfer_type</c> é <c>funding</c>.</summary>
    public string? SendAsDepositBankAccountGuid { get; init; }

    /// <summary>Conta fiat do bank a usar na transfer — obrigatório se o bank tiver múltiplas contas fiat (rails <c>instant_funding</c>/<c>lightning</c>).</summary>
    public string? BankFiatAccountGuid { get; init; }

    /// <summary>Conta fiat do customer a usar na transfer — obrigatório se o customer tiver múltiplas contas fiat (rails <c>instant_funding</c>/<c>lightning</c>).</summary>
    public string? CustomerFiatAccountGuid { get; init; }

    /// <summary>Conta que paga a network fee (lightning) — fiat ou trading do customer/bank.</summary>
    public string? NetworkFeeAccountGuid { get; init; }

    /// <summary>
    /// Comportamento simulado, só em sandbox (<c>force_review</c> força revisão manual em
    /// transfers <c>funding</c>/<c>instant_funding</c>). NUNCA usar em produção.
    /// </summary>
    public IReadOnlyList<string>? ExpectedBehaviours { get; init; }

    public IReadOnlyList<string>? Labels { get; init; }
}

/// <summary>
/// Corpo de <c>PATCH transfers/{guid}</c> — spec oficial (<c>PatchTransfer</c>). Único uso
/// documentado: (re)definir os participantes de uma transfer em andamento. **Sensível** — atualiza
/// uma transferência financeira; nunca exercitado contra sandbox (só contrato/mock).
/// </summary>
public sealed record PatchCybridTransferRequest
{
    public IReadOnlyList<PatchCybridTransferParticipant>? SourceParticipants { get; init; }
    public IReadOnlyList<PatchCybridTransferParticipant>? DestinationParticipants { get; init; }
}

/// <summary>
/// Participante de transfer no PATCH — diferente de <see cref="CybridTransferParticipant"/> (POST):
/// aqui os três campos são obrigatórios pela spec oficial (<c>PatchTransferParticipant</c>).
/// </summary>
public sealed record PatchCybridTransferParticipant
{
    public required string Type { get; init; }
    public required long Amount { get; init; }
    public required string Guid { get; init; }
}

/// <summary>Participante de transfer (<c>type</c>/<c>amount</c> obrigatórios na spec).</summary>
public sealed record CybridTransferParticipant
{
    public required string Type { get; init; }
    public required long Amount { get; init; }
    public string? Guid { get; init; }
}

/// <summary>Transfer — spec oficial v0.129, listagem validada ao vivo (93 transfers reais).</summary>
public sealed record CybridTransfer
{
    public string? Guid { get; init; }
    public string? TransferType { get; init; }
    public string? BankGuid { get; init; }
    public string? CustomerGuid { get; init; }
    public string? QuoteGuid { get; init; }
    public string? ExternalBankAccountGuid { get; init; }
    public string? Asset { get; init; }
    public string? Side { get; init; }

    /// <summary>Valores conhecidos: <c>storing</c>, <c>pending</c>, <c>reviewing</c>, <c>completed</c>, <c>failed</c>, <c>cancelled</c>, <c>reversed</c> (conjunto aberto).</summary>
    public string? State { get; init; }

    public string? FailureCode { get; init; }
    public string? ReturnCode { get; init; }
    public long? Amount { get; init; }
    public long? EstimatedAmount { get; init; }
    public long? Fee { get; init; }
    public long? NetworkFee { get; init; }
    public string? NetworkFeeAsset { get; init; }
    public string? TxnHash { get; init; }
    public string? PaymentRail { get; init; }
    public string? ExternalId { get; init; }
    public string? ReferenceTransferGuid { get; init; }
    public string? DepositAddressGuid { get; init; }

    /// <summary>Objetos livres da spec (shape varia por transfer_type) — expostos crus, sem normalização.</summary>
    public JsonElement? SourceAccount { get; init; }

    public JsonElement? DestinationAccount { get; init; }
    public JsonElement? TransferDetails { get; init; }
    public JsonElement? HoldDetails { get; init; }
    public IReadOnlyList<CybridTransferParticipant>? SourceParticipants { get; init; }
    public IReadOnlyList<CybridTransferParticipant>? DestinationParticipants { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
