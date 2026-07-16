using CambioReal.Cybrid;
using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Http;
using CambioReal.Cybrid.Models;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace CambioReal.Cybrid.SandboxTests;

/// <summary>
/// Integração sandbox real, opt-in — goal-loop §2.5: "nunca rodem por padrão em CI e não
/// imprimam segredos". Credenciais vêm de variáveis de ambiente populadas a partir do
/// <c>pass</c>, nunca hardcoded aqui:
///
/// <code>
/// export CYBRID_SANDBOX_CLIENT_ID="$(pass show cambio-real-v2/cybrid/client-id)"
/// export CYBRID_SANDBOX_CLIENT_SECRET="$(pass show cambio-real-v2/cybrid/client-secret)"
/// export CYBRID_SANDBOX_BANK_GUID="$(pass show cambio-real-v2/cybrid/bank-guid)"
/// dotnet test tests/CambioReal.Cybrid.Client.SandboxTests/CambioReal.Cybrid.Client.SandboxTests.csproj
/// </code>
///
/// Sem as variáveis, os testes falham explicitamente (não pulam em silêncio). A criação de QUOTE
/// (read-like — expira sozinha, não move fundos; §0.4 do goal) exige opt-in adicional:
/// <c>CYBRID_SANDBOX_ALLOW_WRITE=1</c>. Criação de TRADE/TRANSFER (financial-write) NÃO existe
/// aqui por design (goal §0.5). Nenhum teste imprime token, secret ou payload integral.
/// </summary>
public sealed class CybridSandboxTests
{
    private readonly ITestOutputHelper output;

    public CybridSandboxTests(ITestOutputHelper output) => this.output = output;

    [Fact]
    [Trait("Category", "Sandbox")]
    public async Task AuthenticatesLiveAgainstSandbox()
    {
        using var provider = BuildServiceProvider();
        var tokenProvider = provider.GetRequiredService<ICybridTokenProvider>();

        var (token, tokenType) = await tokenProvider.GetAccessTokenAsync(CybridScopes.BanksRead, invalidatedToken: null);

        tokenType.ShouldBe("Bearer");
        token.ShouldNotBeNullOrWhiteSpace();
        output.WriteLine($"sandbox: token issued (banks:read), length={token.Length} (masked).");
    }

    [Fact]
    [Trait("Category", "Sandbox")]
    public async Task BankReadsLive()
    {
        using var provider = BuildServiceProvider();
        var client = provider.GetRequiredService<CybridClient>();

        var bank = await client.Banks.GetAsync();

        bank.Guid.ShouldNotBeNullOrWhiteSpace();
        bank.Name.ShouldNotBeNullOrWhiteSpace();
        output.WriteLine($"GET banks/{{guid}}: 200, name={bank.Name}, trading symbols={bank.SupportedTradingSymbols?.Count}.");
    }

    [Fact]
    [Trait("Category", "Sandbox")]
    public async Task AccountsAndTransfersListLive()
    {
        using var provider = BuildServiceProvider();
        var client = provider.GetRequiredService<CybridClient>();

        var accounts = await client.Accounts.ListAsync(perPage: 3);
        accounts.Total.ShouldBeGreaterThan(0);
        output.WriteLine($"GET accounts: 200, total={accounts.Total}.");

        var transfers = await client.Transfers.ListAsync(perPage: 3);
        transfers.Total.ShouldBeGreaterThan(0);
        output.WriteLine($"GET transfers: 200, total={transfers.Total} (dados reais, leitura pura).");

        if (transfers.Objects.Count > 0 && !string.IsNullOrWhiteSpace(transfers.Objects[0].Guid))
        {
            var transfer = await client.Transfers.GetAsync(transfers.Objects[0].Guid!);
            transfer.Guid.ShouldBe(transfers.Objects[0].Guid);
            output.WriteLine($"GET transfers/{{guid}}: 200, state={transfer.State}.");
        }
    }

    [Fact]
    [Trait("Category", "Sandbox")]
    public async Task PricesReadLive()
    {
        using var provider = BuildServiceProvider();
        var client = provider.GetRequiredService<CybridClient>();

        var prices = await client.Prices.ListAsync("USDC-USD");

        prices.Count.ShouldBeGreaterThan(0);
        prices[0].Symbol.ShouldBe("USDC-USD");
        output.WriteLine($"GET prices: 200, buy={prices[0].BuyPrice}, sell={prices[0].SellPrice} (menor unidade).");
    }

    [Fact]
    [Trait("Category", "Sandbox")]
    public async Task FictitiousGuidReturnsDomainNotFound()
    {
        using var provider = BuildServiceProvider();
        var client = provider.GetRequiredService<CybridClient>();

        var error = await Should.ThrowAsync<CybridApiException>(
            async () => await client.Accounts.GetAsync("00000000000000000000000000000000"));

        error.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        error.ErrorCode.ShouldBe("not_found");
        output.WriteLine("GET accounts/{fictício}: 404, message_code=not_found — acesso real ao recurso confirmado.");
    }

    /// <summary>
    /// Gap P1 fechado: <c>GET identity_verifications</c> (listagem) não existia — só GET por guid.
    /// Leitura pura, sem opt-in adicional (mesma política dos demais <c>*ListLive</c>).
    /// </summary>
    [Fact]
    [Trait("Category", "Sandbox")]
    public async Task IdentityVerificationsListLive()
    {
        using var provider = BuildServiceProvider();
        var client = provider.GetRequiredService<CybridClient>();

        var verifications = await client.IdentityVerifications.ListAsync(perPage: 5);
        output.WriteLine($"GET identity_verifications: 200, total={verifications.Total}.");

        if (verifications.Objects.Count > 0 && !string.IsNullOrWhiteSpace(verifications.Objects[0].Guid))
        {
            var verification = await client.IdentityVerifications.GetAsync(verifications.Objects[0].Guid!);
            verification.Guid.ShouldBe(verifications.Objects[0].Guid);
            output.WriteLine($"GET identity_verifications/{{guid}}: 200, state={verification.State}.");
        }
    }

    /// <summary>
    /// Parciais (list/get filtros) fechados nos recursos já cobertos — leitura pura, sem opt-in
    /// adicional. Confirma que os novos parâmetros de query não quebram a chamada real.
    /// </summary>
    [Fact]
    [Trait("Category", "Sandbox")]
    public async Task ListFiltersWorkAgainstLiveSandbox()
    {
        using var provider = BuildServiceProvider();
        var client = provider.GetRequiredService<CybridClient>();

        var customers = await client.Customers.ListAsync(perPage: 3, type: "individual");
        output.WriteLine($"GET customers?type=individual: 200, total={customers.Total}.");

        var transfers = await client.Transfers.ListAsync(perPage: 3, state: "completed");
        output.WriteLine($"GET transfers?state=completed: 200, total={transfers.Total}.");

        var externalBankAccounts = await client.ExternalBankAccounts.ListAsync(perPage: 3, state: "completed");
        output.WriteLine($"GET external_bank_accounts?state=completed: 200, total={externalBankAccounts.Total}.");
    }

    /// <summary>
    /// Criação de QUOTE trading — read-like pelo goal §0.4 ("quote não vinculante"): expira
    /// sozinha (<c>expires_at</c>), não move fundos; a execução (trade) é que seria financeira e
    /// NÃO acontece aqui. Opt-in duplo: credenciais + <c>CYBRID_SANDBOX_ALLOW_WRITE=1</c>.
    /// </summary>
    [Fact]
    [Trait("Category", "SandboxWrite")]
    public async Task QuoteCreateLiveExpiresAlone()
    {
        if (Environment.GetEnvironmentVariable("CYBRID_SANDBOX_ALLOW_WRITE") != "1")
        {
            output.WriteLine("CYBRID_SANDBOX_ALLOW_WRITE != 1 — criação de quote pulada por design.");
            return;
        }

        using var provider = BuildServiceProvider();
        var client = provider.GetRequiredService<CybridClient>();

        // Usa um customer REAL existente da conta sandbox (não cria nada novo).
        var customers = await client.Customers.ListAsync(perPage: 10);
        var customer = customers.Objects.FirstOrDefault(c => c.State == "verified")
            ?? (customers.Objects.Count > 0 ? customers.Objects[0] : null);
        customer.ShouldNotBeNull("a conta sandbox tem 208 customers — a listagem não pode vir vazia");

        var quote = await client.Quotes.CreateAsync(new CreateCybridQuoteRequest
        {
            ProductType = CybridProductTypes.Trading,
            CustomerGuid = customer!.Guid,
            Symbol = "USDC-USD",
            Side = CybridQuoteSides.Buy,
            // 1 USDC na menor unidade — valor mínimo; a quote nunca é executada.
            ReceiveAmount = 1_000_000,
        });

        quote.Guid.ShouldNotBeNullOrWhiteSpace();
        output.WriteLine(
            $"POST quotes: criado, product=trading USDC-USD, expira em {quote.ExpiresAt:O} — nenhum trade/transfer executado.");
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var clientId = Environment.GetEnvironmentVariable("CYBRID_SANDBOX_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("CYBRID_SANDBOX_CLIENT_SECRET");
        var bankGuid = Environment.GetEnvironmentVariable("CYBRID_SANDBOX_BANK_GUID");

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(bankGuid))
        {
            throw new InvalidOperationException(
                "CYBRID_SANDBOX_CLIENT_ID/CLIENT_SECRET/BANK_GUID ausentes — carregue-os de " +
                "`pass cambio-real-v2/cybrid/*` antes de rodar este projeto. " +
                "Ver o comentário XML no topo de CybridSandboxTests.cs.");
        }

        var services = new ServiceCollection();
        services.AddCybridClient(options =>
        {
            options.Environment = CybridEnvironment.Sandbox;
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.BankGuid = bankGuid;
        });

        return services.BuildServiceProvider();
    }
}
