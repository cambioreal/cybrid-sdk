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
