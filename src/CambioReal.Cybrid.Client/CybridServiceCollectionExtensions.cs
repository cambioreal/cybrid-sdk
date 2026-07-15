using CambioReal.Cybrid.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CambioReal.Cybrid;

/// <summary>Registro do cliente Cybrid no container.</summary>
public static class CybridServiceCollectionExtensions
{
    /// <summary>
    /// Registra o cliente a partir de uma seção de configuração.
    /// </summary>
    /// <remarks>
    /// As credenciais precisam chegar por um provider seguro (variáveis de ambiente, user-secrets,
    /// Vault). Nunca versione <c>ClientId</c>/<c>ClientSecret</c> em <c>appsettings.json</c> — a
    /// fonte da verdade é o <c>pass</c>, <c>cambio-real-v2/cybrid/*</c>.
    /// </remarks>
    public static IServiceCollection AddCybridClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return services.AddCybridClient(configuration.Bind);
    }

    /// <summary>
    /// Registra <see cref="CybridClient"/>, o provedor de token (um token por scope) e os dois
    /// pipelines HTTP: o da Bank API (autenticado, scope por request) e o de auth (sem handler —
    /// host separado, <c>id.*.cybrid.app</c>).
    /// </summary>
    public static IServiceCollection AddCybridClient(this IServiceCollection services, Action<CybridOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.AddOptions<CybridOptions>().Validate(
            options =>
            {
                options.Validate();
                return true;
            },
            "A configuração do CybridOptions é inválida.");

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<ICybridTokenProvider, CybridTokenProvider>();

        services.AddHttpClient(CybridClientNames.Auth, ConfigureAuthTransport);

        services.AddHttpClient(CybridClientNames.Api, ConfigureApiTransport)
            .AddHttpMessageHandler(provider =>
                new CybridAuthenticationHandler(provider.GetRequiredService<ICybridTokenProvider>()));

        services.TryAddTransient(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new CybridClient(
                factory.CreateClient(CybridClientNames.Api),
                provider.GetRequiredService<IOptions<CybridOptions>>());
        });

        return services;
    }

    private static void ConfigureApiTransport(IServiceProvider provider, HttpClient client)
    {
        var options = GetValidatedOptions(provider);

        client.BaseAddress = options.ResolveBaseAddress();
        client.Timeout = options.Timeout;

        // Pin de versão da API (accept-version) — paridade com o legado (2025-10-01).
        if (!string.IsNullOrWhiteSpace(options.ApiVersion))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("accept-version", options.ApiVersion);
        }
    }

    private static void ConfigureAuthTransport(IServiceProvider provider, HttpClient client)
    {
        var options = GetValidatedOptions(provider);

        client.BaseAddress = options.ResolveAuthBaseAddress();
        client.Timeout = options.Timeout;
    }

    private static CybridOptions GetValidatedOptions(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<CybridOptions>>().Value;
        options.Validate();
        return options;
    }
}
