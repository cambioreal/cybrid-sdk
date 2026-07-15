namespace CambioReal.Cybrid.Auth;

/// <summary>Fornece access tokens OAuth2 da Cybrid, um por scope, com cache e renovação sob demanda.</summary>
public interface ICybridTokenProvider
{
    /// <summary>
    /// Devolve um token válido para <paramref name="scope"/>, do cache ou renovado.
    /// <paramref name="invalidatedToken"/> força a renovação quando o token informado (recebido
    /// num 401) ainda for o corrente — mecanismo do retry único do <c>CybridAuthenticationHandler</c>.
    /// </summary>
    public ValueTask<(string Token, string TokenType)> GetAccessTokenAsync(
        string scope, string? invalidatedToken, CancellationToken cancellationToken = default);
}
