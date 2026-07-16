using System.Text.Json;

namespace CambioReal.Cybrid.Models;

/// <summary>
/// Corpo de <c>POST external_bank_accounts</c> — spec oficial (<c>name</c>/<c>account_kind</c>
/// obrigatórios; via Plaid ou <c>raw_routing_details</c> com dados da contraparte).
/// </summary>
public sealed record CreateCybridExternalBankAccountRequest
{
    public required string Name { get; init; }

    /// <summary>Valores da spec: <c>plaid</c>, <c>plaid_processor_token</c>, <c>raw_routing_details</c>.</summary>
    public required string AccountKind { get; init; }

    public string? CustomerGuid { get; init; }
    public string? CounterpartyGuid { get; init; }
    public string? Asset { get; init; }
    public string? PlaidPublicToken { get; init; }
    public string? PlaidAccountId { get; init; }
    public string? PlaidProcessorToken { get; init; }

    /// <summary>Institution ID do Plaid — obrigatório quando <see cref="AccountKind"/> é <c>plaid_processor_token</c>.</summary>
    public string? PlaidInstitutionId { get; init; }

    /// <summary>Máscara da conta (últimos dígitos) — obrigatório quando <see cref="AccountKind"/> é <c>plaid_processor_token</c>.</summary>
    public string? PlaidAccountMask { get; init; }

    /// <summary>Nome da conta no Plaid — obrigatório quando <see cref="AccountKind"/> é <c>plaid_processor_token</c>.</summary>
    public string? PlaidAccountName { get; init; }

    public JsonElement? CounterpartyBankAccount { get; init; }

    /// <summary>
    /// Forma usada pelo legado para ACH raw routing (<c>counterparty_bank_account_details</c>:
    /// <c>{payment_rail, bank_code_type: "ABA", bank_code, account_identifier}</c>) — nome de
    /// campo distinto do <c>counterparty_bank_account</c> da spec v0.129; ambos expostos.
    /// </summary>
    public JsonElement? CounterpartyBankAccountDetails { get; init; }
    public CybridName? CounterpartyName { get; init; }
    public CybridAddress? CounterpartyAddress { get; init; }
    public string? CounterpartyEmailAddress { get; init; }
}

/// <summary>
/// Corpo de <c>PATCH external_bank_accounts/{guid}</c> — spec oficial (<c>PatchExternalBankAccount</c>).
/// Único campo aceito é <c>state</c>, obrigatório, com dois valores possíveis. Ver
/// <see cref="CybridExternalBankAccountPatchStates"/>.
/// </summary>
public sealed record PatchCybridExternalBankAccountRequest
{
    /// <summary>
    /// Se <c>completed</c>: a API devolve <c>completed</c> (se a conta já foi verificada) ou
    /// <c>unverified</c> (senão). Valores da spec: <c>completed</c>, <c>refresh_required</c>.
    /// </summary>
    public required string State { get; init; }
}

/// <summary>Valores aceitos por <see cref="PatchCybridExternalBankAccountRequest.State"/> (spec oficial: enum fechado).</summary>
public static class CybridExternalBankAccountPatchStates
{
    public const string Completed = "completed";
    public const string RefreshRequired = "refresh_required";
}

/// <summary>External bank account — spec oficial v0.129, listagem validada ao vivo (131 reais).</summary>
public sealed record CybridExternalBankAccount
{
    public string? Guid { get; init; }
    public string? Name { get; init; }
    public string? Asset { get; init; }
    public string? AccountKind { get; init; }
    public string? Environment { get; init; }
    public string? BankGuid { get; init; }
    public string? CustomerGuid { get; init; }
    public string? CounterpartyGuid { get; init; }

    /// <summary>Valores conhecidos: <c>storing</c>, <c>completed</c>, <c>failed</c>, <c>refresh_required</c>, <c>unverified</c>, <c>deleting</c>, <c>deleted</c> (conjunto aberto).</summary>
    public string? State { get; init; }

    public string? FailureCode { get; init; }
    public string? PlaidInstitutionId { get; init; }
    public string? PlaidAccountMask { get; init; }
    public string? PlaidAccountName { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
