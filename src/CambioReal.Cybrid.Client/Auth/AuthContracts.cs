using System.Text.Json.Serialization;

namespace CambioReal.Cybrid.Auth;

/// <summary>
/// Corpo de <c>POST {auth}/oauth/token</c> — JSON com <c>scope</c> por operação, confirmado no
/// legado (<c>AbstractService::authenticate()</c>) e validado ao vivo (2026-07-15, HTTP 200 com
/// scopes ecoados). Host de auth é separado da Bank API (<c>id.*.cybrid.app</c>).
/// </summary>
internal sealed record CybridTokenRequest(
    [property: JsonPropertyName("grant_type")] string GrantType,
    [property: JsonPropertyName("client_id")] string ClientId,
    [property: JsonPropertyName("client_secret")] string ClientSecret,
    [property: JsonPropertyName("scope")] string Scope);

/// <summary>
/// Resposta de <c>POST {auth}/oauth/token</c>: <c>{access_token, token_type: "Bearer",
/// expires_in: 28799, scope, created_at}</c> — validado ao vivo (2026-07-15).
/// </summary>
internal sealed record CybridTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("scope")] string? Scope);

/// <summary>Token em cache com seu instante de expiração absoluto.</summary>
internal sealed record CachedAccessToken(string Value, string TokenType, DateTimeOffset ExpiresAtUtc);

/// <summary>Nomes dos <c>HttpClient</c> registrados no container.</summary>
internal static class CybridClientNames
{
    /// <summary>Cliente da Bank API. Passa pelo <c>CybridAuthenticationHandler</c> (scope por request).</summary>
    public const string Api = "cybrid.api";

    /// <summary>
    /// Cliente usado só para <c>POST oauth/token</c>. Separado por dois motivos: (1) sem handler
    /// de autenticação, para não recorrer; (2) host distinto (<c>id.*.cybrid.app</c>).
    /// </summary>
    public const string Auth = "cybrid.auth";
}

/// <summary>
/// Scopes OAuth2 da Cybrid usados pelo SDK — um token por scope (a Cybrid emite tokens restritos;
/// o legado autentica por operação com cache por scope). Vocabulário confirmado no legado e na
/// spec oficial: <c>{recurso}:{read|execute}</c>.
/// </summary>
public static class CybridScopes
{
    public const string BanksRead = "banks:read";
    public const string AccountsRead = "accounts:read";
    public const string AccountsExecute = "accounts:execute";
    public const string CustomersRead = "customers:read";
    public const string CustomersExecute = "customers:execute";
    public const string CounterpartiesRead = "counterparties:read";
    public const string CounterpartiesExecute = "counterparties:execute";
    public const string ExternalBankAccountsRead = "external_bank_accounts:read";
    public const string ExternalBankAccountsExecute = "external_bank_accounts:execute";
    public const string DepositBankAccountsRead = "deposit_bank_accounts:read";
    public const string DepositBankAccountsExecute = "deposit_bank_accounts:execute";
    public const string IdentityVerificationsRead = "identity_verifications:read";
    public const string IdentityVerificationsExecute = "identity_verifications:execute";
    public const string QuotesRead = "quotes:read";
    public const string QuotesExecute = "quotes:execute";
    public const string TradesRead = "trades:read";
    public const string TradesExecute = "trades:execute";
    public const string TransfersRead = "transfers:read";
    public const string TransfersExecute = "transfers:execute";
    public const string PricesRead = "prices:read";
}
