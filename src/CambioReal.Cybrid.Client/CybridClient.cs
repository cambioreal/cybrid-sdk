using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Http;
using CambioReal.Cybrid.Resources;
using CambioReal.Cybrid.Serialization;
using Microsoft.Extensions.Options;

namespace CambioReal.Cybrid;

/// <summary>
/// Cliente HTTP da Cybrid Bank API.
/// </summary>
/// <remarks>
/// Camada de transporte, no mesmo espírito do <c>RippleClient</c>/<c>Bs2Client</c>/<c>BexsClient</c>.
/// Um único <see cref="HttpClient"/> autenticado; o scope OAuth de cada operação viaja por
/// requisição via <see cref="CybridAuthenticationHandler.ScopeOption"/> (a Cybrid emite um token
/// por scope — ver <c>CybridTokenProvider</c>). Não há header de idempotência documentado no
/// legado; a spec oficial não o expõe nos POSTs usados — correlação de negócio via labels/guids.
/// </remarks>
public sealed class CybridClient
{
    private readonly HttpClient httpClient;

    /// <summary>Cria o cliente sobre um <see cref="HttpClient"/> já configurado e autenticado.</summary>
    public CybridClient(HttpClient httpClient, IOptions<CybridOptions> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        this.httpClient = httpClient;
        BankGuid = options.Value.BankGuid;

        Banks = new BanksResource(this);
        Accounts = new AccountsResource(this);
        Customers = new CustomersResource(this);
        Counterparties = new CounterpartiesResource(this);
        IdentityVerifications = new IdentityVerificationsResource(this);
        ExternalBankAccounts = new ExternalBankAccountsResource(this);
        DepositBankAccounts = new DepositBankAccountsResource(this);
        Quotes = new QuotesResource(this);
        Trades = new TradesResource(this);
        Transfers = new TransfersResource(this);
        Prices = new PricesResource(this);
    }

    /// <summary>GUID do bank alvo — injetado nas listagens/criações que o exigem.</summary>
    public string BankGuid { get; }

    /// <summary>Bank. <c>banks</c>.</summary>
    public BanksResource Banks { get; }

    /// <summary>Contas internas (fiat/trading). <c>accounts</c>.</summary>
    public AccountsResource Accounts { get; }

    /// <summary>Clientes (KYC). <c>customers</c>.</summary>
    public CustomersResource Customers { get; }

    /// <summary>Contrapartes (beneficiários de payout). <c>counterparties</c>.</summary>
    public CounterpartiesResource Counterparties { get; }

    /// <summary>Verificações de identidade (KYC/KYB/contraparte). <c>identity_verifications</c>.</summary>
    public IdentityVerificationsResource IdentityVerifications { get; }

    /// <summary>Contas bancárias externas. <c>external_bank_accounts</c>.</summary>
    public ExternalBankAccountsResource ExternalBankAccounts { get; }

    /// <summary>Contas de depósito. <c>deposit_bank_accounts</c>.</summary>
    public DepositBankAccountsResource DepositBankAccounts { get; }

    /// <summary>Cotações. <c>quotes</c>.</summary>
    public QuotesResource Quotes { get; }

    /// <summary>Trades (execução de cotações trading). <c>trades</c>.</summary>
    public TradesResource Trades { get; }

    /// <summary>Transferências (funding/crypto/book). <c>transfers</c>.</summary>
    public TransfersResource Transfers { get; }

    /// <summary>Preços spot. <c>prices</c>.</summary>
    public PricesResource Prices { get; }

    internal async Task<TResponse> GetAsync<TResponse>(string path, string scope, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Get, path, scope, content: null);
        return await SendAndReadAsync<TResponse>(request, cancellationToken);
    }

    internal async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path, TRequest body, string scope, CancellationToken cancellationToken)
    {
        var content = JsonContent.Create(body, options: CybridJson.Options);
        using var request = CreateRequest(HttpMethod.Post, path, scope, content);
        return await SendAndReadAsync<TResponse>(request, cancellationToken);
    }

    internal async Task<TResponse> DeleteAsync<TResponse>(string path, string scope, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Delete, path, scope, content: null);
        return await SendAndReadAsync<TResponse>(request, cancellationToken);
    }

    internal async Task<TResponse> PatchAsync<TRequest, TResponse>(
        string path, TRequest body, string scope, CancellationToken cancellationToken)
    {
        var content = JsonContent.Create(body, options: CybridJson.Options);
        using var request = CreateRequest(HttpMethod.Patch, path, scope, content);
        return await SendAndReadAsync<TResponse>(request, cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string path, string scope, HttpContent? content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);

        if (path.StartsWith('/'))
        {
            throw new ArgumentException(
                $"O path deve ser relativo e não pode começar com '/'. Recebido: '{path}'.",
                nameof(path));
        }

        var request = new HttpRequestMessage(method, new Uri(path, UriKind.Relative))
        {
            Content = content,
        };

        request.Options.Set(CybridAuthenticationHandler.ScopeOption, scope);

        return request;
    }

    private async Task<TResponse> SendAndReadAsync<TResponse>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await ThrowIfUnsuccessfulAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<TResponse>(CybridJson.Options, cancellationToken);

        return payload ?? throw new CybridApiException(
            response.StatusCode,
            errorCode: null,
            "A Cybrid devolveu um corpo JSON vazio onde um valor era esperado.",
            responseBody: null);
    }

    private static async Task ThrowIfUnsuccessfulAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var (messageCode, errorMessage) = TryExtractError(body);
        var message = $"A Cybrid respondeu HTTP {(int)response.StatusCode} ({response.StatusCode})"
            + (errorMessage is null ? "." : $": {errorMessage}.");

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new CybridAuthenticationException(response.StatusCode, messageCode, message, body),
            _ => new CybridApiException(response.StatusCode, messageCode, message, body),
        };
    }

    /// <summary>
    /// Extrai <c>message_code</c>/<c>error_message</c> do corpo de erro — forma confirmada ao vivo
    /// (2026-07-15): <c>{"status":404,"error_message":"Record not found","message_code":"not_found"}</c>.
    /// </summary>
    private static (string? Code, string? Message) TryExtractError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return (null, null);
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return (null, null);
            }

            string? code = null;
            string? message = null;

            if (root.TryGetProperty("message_code", out var codeElement) && codeElement.ValueKind == JsonValueKind.String)
            {
                code = codeElement.GetString();
            }

            if (root.TryGetProperty("error_message", out var messageElement) && messageElement.ValueKind == JsonValueKind.String)
            {
                message = messageElement.GetString();
            }

            return (code, message);
        }
        catch (JsonException)
        {
            // Corpo não-JSON — status e corpo bruto já vão na exceção.
            return (null, null);
        }
    }
}
