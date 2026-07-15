using System.Net;
using System.Net.Http.Headers;
using CambioReal.Cybrid.Http;

namespace CambioReal.Cybrid.Auth;

/// <summary>
/// Injeta <c>Authorization: {token_type} {access_token}</c> usando o token do scope declarado
/// pela requisição (via <see cref="ScopeOption"/>), reautenticando uma única vez diante de 401.
/// </summary>
/// <remarks>
/// Diferente da BS2 (2 scopes fixos ⇒ 2 <c>HttpClient</c>), a Cybrid tem um scope por
/// recurso+verbo — um único pipeline HTTP com o scope viajando por request em
/// <see cref="HttpRequestMessage.Options"/> evita uma explosão de clientes nomeados.
/// </remarks>
internal sealed class CybridAuthenticationHandler : DelegatingHandler
{
    /// <summary>Chave do scope OAuth em <see cref="HttpRequestMessage.Options"/> — setada pelo <c>CybridClient</c>.</summary>
    public static readonly HttpRequestOptionsKey<string> ScopeOption = new("cybrid.scope");

    private readonly ICybridTokenProvider tokenProvider;

    public CybridAuthenticationHandler(ICybridTokenProvider tokenProvider)
    {
        ArgumentNullException.ThrowIfNull(tokenProvider);
        this.tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.Options.TryGetValue(ScopeOption, out var scope) || string.IsNullOrWhiteSpace(scope))
        {
            throw new InvalidOperationException(
                "Requisição Cybrid sem scope OAuth declarado — o CybridClient deve setar " +
                "CybridAuthenticationHandler.ScopeOption em toda requisição.");
        }

        var (token, tokenType) = await tokenProvider.GetAccessTokenAsync(scope, invalidatedToken: null, cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue(tokenType, token);

        // A cópia precisa existir antes do envio — depois dele o Content já foi descartado.
        var retry = await request.CloneAsync(cancellationToken);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            retry.Dispose();
            return response;
        }

        response.Dispose();

        var (refreshedToken, refreshedTokenType) = await tokenProvider.GetAccessTokenAsync(scope, invalidatedToken: token, cancellationToken);
        retry.Headers.Authorization = new AuthenticationHeaderValue(refreshedTokenType, refreshedToken);

        return await base.SendAsync(retry, cancellationToken);
    }
}
