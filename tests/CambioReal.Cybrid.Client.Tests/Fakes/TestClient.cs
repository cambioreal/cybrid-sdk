using System.Net;
using CambioReal.Cybrid.Auth;
using Microsoft.Extensions.Options;

namespace CambioReal.Cybrid.Tests.Fakes;

internal static class TestClient
{
    public static CybridOptions NewOptions() => new()
    {
        Environment = CybridEnvironment.Sandbox,
        ClientId = "client-1",
        ClientSecret = "secret-1",
        BankGuid = "bank-guid-1",
    };

    /// <summary>
    /// Monta um <see cref="CybridClient"/> sobre um transporte gravado, com token fixo — o
    /// pipeline real (<c>CybridAuthenticationHandler</c> → transporte), só sem rede. O stub grava
    /// os scopes solicitados para asserção.
    /// </summary>
    public static (CybridClient Client, RecordingHttpMessageHandler Transport, StubTokenProvider Tokens) Create(
        params (HttpStatusCode Status, string Json)[] responses)
    {
        var transport = new RecordingHttpMessageHandler();

        foreach (var (status, json) in responses)
        {
            transport.RespondWith(status, json);
        }

        var options = NewOptions();
        var tokens = new StubTokenProvider("tok-1");

        var handler = new CybridAuthenticationHandler(tokens)
        {
            InnerHandler = transport,
        };

        var httpClient = new HttpClient(handler) { BaseAddress = options.ResolveBaseAddress() };

        return (new CybridClient(httpClient, Options.Create(options)), transport, tokens);
    }

    public static (CybridClient Client, RecordingHttpMessageHandler Transport, StubTokenProvider Tokens) CreateOk(string json = "{}") =>
        Create((HttpStatusCode.OK, json));
}
