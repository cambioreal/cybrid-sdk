namespace CambioReal.Cybrid;

/// <summary>Ambiente da Cybrid Bank API.</summary>
public enum CybridEnvironment
{
    /// <summary>Sandbox — <c>https://bank.sandbox.cybrid.app/api/</c>.</summary>
    Sandbox = 0,

    /// <summary>Produção — <c>https://bank.production.cybrid.app/api/</c>.</summary>
    Production = 1,
}

/// <summary>Resolve os endereços de cada <see cref="CybridEnvironment"/>.</summary>
public static class CybridEnvironmentExtensions
{
    /// <summary>
    /// Endereço base da Bank API, confirmado em <c>cerebro/config/cybrid.php</c>
    /// (<c>connections.demo.url</c>/<c>connections.production.url</c>) e validado ao vivo contra o
    /// sandbox em 2026-07-15 (banks/accounts/transfers/quotes reais — discovery.md §4).
    /// </summary>
    public static Uri GetBaseAddress(this CybridEnvironment environment) => environment switch
    {
        CybridEnvironment.Production => new Uri("https://bank.production.cybrid.app/api/", UriKind.Absolute),
        CybridEnvironment.Sandbox => new Uri("https://bank.sandbox.cybrid.app/api/", UriKind.Absolute),
        _ => throw new ArgumentOutOfRangeException(nameof(environment), environment, "Ambiente Cybrid desconhecido."),
    };

    /// <summary>
    /// Endereço da API de autenticação — host SEPARADO da Bank API
    /// (<c>id.sandbox.cybrid.app</c>/<c>id.production.cybrid.app</c>), confirmado no legado
    /// (<c>connections.*.auth_url</c>).
    /// </summary>
    public static Uri GetAuthBaseAddress(this CybridEnvironment environment) => environment switch
    {
        CybridEnvironment.Production => new Uri("https://id.production.cybrid.app/", UriKind.Absolute),
        CybridEnvironment.Sandbox => new Uri("https://id.sandbox.cybrid.app/", UriKind.Absolute),
        _ => throw new ArgumentOutOfRangeException(nameof(environment), environment, "Ambiente Cybrid desconhecido."),
    };
}
