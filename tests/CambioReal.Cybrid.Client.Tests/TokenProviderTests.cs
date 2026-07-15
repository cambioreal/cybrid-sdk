using System.Net;
using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Http;
using CambioReal.Cybrid.Tests.Fakes;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace CambioReal.Cybrid.Tests;

public sealed class TokenProviderTests
{
    private static readonly DateTimeOffset Epoch = new(2026, 7, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AuthenticatesOnceAndCachesTheTokenPerScope()
    {
        var (provider, transport) = Build(new MutableTimeProvider(Epoch), TokenResponse("tok-1"));

        (await provider.GetAccessTokenAsync(CybridScopes.AccountsRead, null)).Token.ShouldBe("tok-1");
        (await provider.GetAccessTokenAsync(CybridScopes.AccountsRead, null)).Token.ShouldBe("tok-1");

        transport.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task AuthRequestIsJsonWithScopeAndWithoutBasicHeader()
    {
        var (provider, transport) = Build(new MutableTimeProvider(Epoch), TokenResponse("tok-1"));

        await provider.GetAccessTokenAsync(CybridScopes.CustomersExecute, null);

        var request = transport.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Post);

        // Host de auth SEPARADO da Bank API — id.sandbox.cybrid.app (config do legado).
        request.RequestUri!.ToString().ShouldBe("https://id.sandbox.cybrid.app/oauth/token");
        request.Authorization.ShouldBeNull();
        request.ContentType.ShouldBe("application/json");
        request.Body!.ShouldContain("\"grant_type\":\"client_credentials\"");
        request.Body!.ShouldContain("\"scope\":\"customers:execute\"");
    }

    [Fact]
    public async Task DifferentScopesAuthenticateAndCacheIndependently()
    {
        var (provider, transport) = Build(
            new MutableTimeProvider(Epoch), TokenResponse("tok-read"), TokenResponse("tok-execute"));

        (await provider.GetAccessTokenAsync(CybridScopes.TransfersRead, null)).Token.ShouldBe("tok-read");
        (await provider.GetAccessTokenAsync(CybridScopes.TransfersExecute, null)).Token.ShouldBe("tok-execute");

        transport.Requests.Count.ShouldBe(2);
        transport.Requests[0].Body!.ShouldContain("transfers:read");
        transport.Requests[1].Body!.ShouldContain("transfers:execute");

        // Reusa o cache por scope — nenhuma chamada extra.
        (await provider.GetAccessTokenAsync(CybridScopes.TransfersRead, null)).Token.ShouldBe("tok-read");
        transport.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task RenewsAfterExpiryMinusSkew()
    {
        var clock = new MutableTimeProvider(Epoch);
        var (provider, transport) = Build(clock, TokenResponse("tok-1"), TokenResponse("tok-2"));

        (await provider.GetAccessTokenAsync(CybridScopes.BanksRead, null)).Token.ShouldBe("tok-1");

        // expires_in = 28799 (validado ao vivo), skew = 60 → válido até Epoch + 28739s.
        clock.Advance(TimeSpan.FromSeconds(28738));
        (await provider.GetAccessTokenAsync(CybridScopes.BanksRead, null)).Token.ShouldBe("tok-1");

        clock.Advance(TimeSpan.FromSeconds(2));
        (await provider.GetAccessTokenAsync(CybridScopes.BanksRead, null)).Token.ShouldBe("tok-2");

        transport.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task InvalidatedTokenForcesRenewalEvenIfNotExpired()
    {
        var (provider, transport) = Build(new MutableTimeProvider(Epoch), TokenResponse("tok-1"), TokenResponse("tok-2"));

        (await provider.GetAccessTokenAsync(CybridScopes.BanksRead, null)).Token.ShouldBe("tok-1");
        (await provider.GetAccessTokenAsync(CybridScopes.BanksRead, "tok-1")).Token.ShouldBe("tok-2");

        transport.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ConcurrentInvalidationsShareASingleRefresh()
    {
        var (provider, transport) = Build(new MutableTimeProvider(Epoch), TokenResponse("tok-1"), TokenResponse("tok-2"));

        await provider.GetAccessTokenAsync(CybridScopes.BanksRead, null);

        var refreshed = await Task.WhenAll(
            Enumerable.Range(0, 8).Select(_ => provider.GetAccessTokenAsync(CybridScopes.BanksRead, "tok-1").AsTask()));

        refreshed.ShouldAllBe(result => result.Token == "tok-2");
        transport.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task FailedAuthenticationThrows()
    {
        var transport = new RecordingHttpMessageHandler();
        transport.RespondWith(HttpStatusCode.Unauthorized, """{"error":"invalid_client"}""");

        var provider = NewProvider(transport, TestClient.NewOptions(), new MutableTimeProvider(Epoch));

        var error = await Should.ThrowAsync<CybridAuthenticationException>(
            async () => await provider.GetAccessTokenAsync(CybridScopes.BanksRead, null));

        error.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static string TokenResponse(string token, int expiresIn = 28799) =>
        $$"""{"access_token":"{{token}}","token_type":"Bearer","expires_in":{{expiresIn}},"scope":"banks:read"}""";

    private static (ICybridTokenProvider Provider, RecordingHttpMessageHandler Transport) Build(
        TimeProvider clock,
        params string[] responses)
    {
        var transport = new RecordingHttpMessageHandler();

        foreach (var response in responses)
        {
            transport.RespondWith(HttpStatusCode.OK, response);
        }

        return (NewProvider(transport, TestClient.NewOptions(), clock), transport);
    }

    private static CybridTokenProvider NewProvider(RecordingHttpMessageHandler transport, CybridOptions options, TimeProvider clock) =>
        new(new SingleHandlerHttpClientFactory(transport, options.ResolveAuthBaseAddress()), Options.Create(options), clock);
}
