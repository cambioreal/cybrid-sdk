using System.Collections.Concurrent;
using System.Net.Http.Json;
using CambioReal.Cybrid.Http;
using CambioReal.Cybrid.Serialization;
using Microsoft.Extensions.Options;

namespace CambioReal.Cybrid.Auth;

/// <summary>
/// Cacheia os tokens OAuth2 da Cybrid (um por scope) e os renova sob demanda.
/// </summary>
/// <remarks>
/// Espelha o <c>Bs2TokenProvider</c> do bs2-sdk (cache/gate chaveados por scope), com scopes
/// como string livre — a Cybrid tem um scope por recurso+verbo (<c>customers:execute</c>,
/// <c>transfers:read</c>, …), confirmado no legado (<c>AbstractService::authenticate()</c>, cache
/// por scope) e ao vivo. Singleton, single-flight por scope; expiração deriva do
/// <c>expires_in</c> real (~8h no sandbox) menos <see cref="CybridOptions.TokenExpirationSkew"/> —
/// o legado cacheava 29min fixos.
/// </remarks>
internal sealed class CybridTokenProvider : ICybridTokenProvider, IDisposable
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly CybridOptions options;
    private readonly TimeProvider timeProvider;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> refreshGates = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, CachedAccessToken> cachedTokens = new(StringComparer.Ordinal);

    public CybridTokenProvider(IHttpClientFactory httpClientFactory, IOptions<CybridOptions> options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);

        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
        this.timeProvider = timeProvider;
    }

    public async ValueTask<(string Token, string TokenType)> GetAccessTokenAsync(
        string scope, string? invalidatedToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);

        if (TryUseCached(scope, invalidatedToken, out var token, out var tokenType))
        {
            return (token, tokenType);
        }

        var gate = refreshGates.GetOrAdd(scope, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (TryUseCached(scope, invalidatedToken, out token, out tokenType))
            {
                return (token, tokenType);
            }

            var fresh = await RequestTokenAsync(scope, cancellationToken);
            cachedTokens[scope] = fresh;
            return (fresh.Value, fresh.TokenType);
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose()
    {
        foreach (var gate in refreshGates.Values)
        {
            gate.Dispose();
        }
    }

    private bool TryUseCached(string scope, string? invalidatedToken, out string token, out string tokenType)
    {
        token = string.Empty;
        tokenType = string.Empty;

        if (!cachedTokens.TryGetValue(scope, out var current))
        {
            return false;
        }

        if (invalidatedToken is not null && string.Equals(current.Value, invalidatedToken, StringComparison.Ordinal))
        {
            return false;
        }

        if (timeProvider.GetUtcNow() >= current.ExpiresAtUtc)
        {
            return false;
        }

        token = current.Value;
        tokenType = current.TokenType;
        return true;
    }

    private async Task<CachedAccessToken> RequestTokenAsync(string scope, CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient(CybridClientNames.Auth);

        // Confirmado no legado e ao vivo: corpo JSON com scope, credenciais SEMPRE no corpo —
        // sem Authorization: Basic.
        var body = new CybridTokenRequest("client_credentials", options.ClientId, options.ClientSecret, scope);

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("oauth/token", UriKind.Relative))
        {
            Content = JsonContent.Create(body, options: CybridJson.Options),
        };

        var issuedAt = timeProvider.GetUtcNow();

        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new CybridAuthenticationException(
                response.StatusCode,
                errorCode: null,
                $"Falha ao autenticar na Cybrid para o scope '{scope}' (HTTP {(int)response.StatusCode}).",
                errorBody);
        }

        var payload = await response.Content.ReadFromJsonAsync<CybridTokenResponse>(CybridJson.Options, cancellationToken)
            ?? throw new CybridAuthenticationException("A Cybrid devolveu um corpo vazio em POST oauth/token.");

        if (string.IsNullOrWhiteSpace(payload.AccessToken))
        {
            throw new CybridAuthenticationException("A Cybrid devolveu um access_token vazio.");
        }

        var lifetime = TimeSpan.FromSeconds(payload.ExpiresIn);
        var skew = options.TokenExpirationSkew;

        if (skew >= lifetime)
        {
            skew = TimeSpan.FromTicks(lifetime.Ticks / 2);
        }

        var tokenType = string.IsNullOrWhiteSpace(payload.TokenType) ? "Bearer" : payload.TokenType;

        return new CachedAccessToken(payload.AccessToken, tokenType, issuedAt + lifetime - skew);
    }
}
