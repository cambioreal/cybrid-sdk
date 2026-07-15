using CambioReal.Cybrid.Auth;

namespace CambioReal.Cybrid.Tests.Fakes;

/// <summary>Token fixo, sem chamada de rede — grava os scopes solicitados para asserção.</summary>
internal sealed class StubTokenProvider(string token, string tokenType = "Bearer") : ICybridTokenProvider
{
    public List<string> RequestedScopes { get; } = [];

    public ValueTask<(string Token, string TokenType)> GetAccessTokenAsync(
        string scope, string? invalidatedToken, CancellationToken cancellationToken = default)
    {
        RequestedScopes.Add(scope);
        return ValueTask.FromResult((token, tokenType));
    }
}
