namespace CambioReal.Cybrid;

/// <summary>
/// Paths centralizados da Cybrid Bank API (relativos a <c>.../api/</c>). Confirmados na spec
/// OpenAPI oficial v0.129 e no legado (<c>cerebro/app/Libraries/Cybrid/*</c>).
/// </summary>
internal static class CybridPaths
{
    public static string Bank(string bankGuid) => $"banks/{Uri.EscapeDataString(bankGuid)}";

    public const string Accounts = "accounts";
    public static string Account(string guid) => $"accounts/{Uri.EscapeDataString(guid)}";

    public const string Customers = "customers";
    public static string Customer(string guid) => $"customers/{Uri.EscapeDataString(guid)}";

    public const string Counterparties = "counterparties";
    public static string Counterparty(string guid) => $"counterparties/{Uri.EscapeDataString(guid)}";

    public const string IdentityVerifications = "identity_verifications";
    public static string IdentityVerification(string guid) => $"identity_verifications/{Uri.EscapeDataString(guid)}";

    public const string ExternalBankAccounts = "external_bank_accounts";
    public static string ExternalBankAccount(string guid) => $"external_bank_accounts/{Uri.EscapeDataString(guid)}";

    public const string DepositBankAccounts = "deposit_bank_accounts";
    public static string DepositBankAccount(string guid) => $"deposit_bank_accounts/{Uri.EscapeDataString(guid)}";

    public const string Quotes = "quotes";
    public static string Quote(string guid) => $"quotes/{Uri.EscapeDataString(guid)}";

    public const string Trades = "trades";
    public static string Trade(string guid) => $"trades/{Uri.EscapeDataString(guid)}";

    public const string Transfers = "transfers";
    public static string Transfer(string guid) => $"transfers/{Uri.EscapeDataString(guid)}";

    public const string Prices = "prices";

    /// <summary>Query de listagem padrão (<c>bank_guid</c> + paginação 0-based).</summary>
    public static string List(string resource, string bankGuid, int page, int perPage, string? extra = null) =>
        $"{resource}?bank_guid={Uri.EscapeDataString(bankGuid)}&page={page}&per_page={perPage}"
        + (string.IsNullOrEmpty(extra) ? string.Empty : $"&{extra}");

    /// <summary>
    /// Monta a query extra de uma listagem a partir de filtros opcionais nomeados (nome já em
    /// <c>snake_case</c>, valor URL-escapado). Filtros com valor nulo/vazio são omitidos. Devolve
    /// <see langword="null"/> se nenhum filtro foi informado — mesma convenção do parâmetro
    /// <c>extra</c> de <see cref="List"/>.
    /// </summary>
    public static string? Filters(params (string Name, string? Value)[] filters)
    {
        List<string>? parts = null;

        foreach (var (name, value) in filters)
        {
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            parts ??= [];
            parts.Add($"{name}={Uri.EscapeDataString(value)}");
        }

        return parts is null ? null : string.Join('&', parts);
    }

    /// <summary>Formata um filtro booleano de query como <c>"true"</c>/<c>"false"</c> (lowercase) — convenção da spec oficial.</summary>
    public static string? Bool(bool? value) => value switch
    {
        null => null,
        true => "true",
        false => "false",
    };

    /// <summary>Formata um filtro de data/hora como ISO8601 — convenção dos filtros <c>*_at_gte</c>/<c>*_at_lt</c> da spec oficial.</summary>
    public static string? Iso(DateTimeOffset? value) => value?.ToString("O");
}
