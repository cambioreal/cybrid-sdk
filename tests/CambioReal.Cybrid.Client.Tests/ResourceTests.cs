using System.Net;
using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Http;
using CambioReal.Cybrid.Models;
using CambioReal.Cybrid.Tests.Fakes;
using Shouldly;
using Xunit;

namespace CambioReal.Cybrid.Tests;

public sealed class ResourceTests
{
    [Fact]
    public async Task BankGetUsesConfiguredGuidAndBanksReadScope()
    {
        var (client, transport, tokens) = TestClient.CreateOk("""{"guid":"bank-guid-1","name":"CambioReal Inc KYC"}""");

        var bank = await client.Banks.GetAsync();

        bank.Name.ShouldBe("CambioReal Inc KYC");
        transport.Requests.Single().RequestUri!.ToString()
            .ShouldBe("https://bank.sandbox.cybrid.app/api/banks/bank-guid-1");
        transport.Requests.Single().Authorization.ShouldBe("Bearer tok-1");
        tokens.RequestedScopes.ShouldBe([CybridScopes.BanksRead]);
    }

    [Fact]
    public async Task AccountsListInjectsBankGuidAndPagination()
    {
        var (client, transport, tokens) = TestClient.CreateOk("""{"total":0,"page":0,"per_page":20,"objects":[]}""");

        var page = await client.Accounts.ListAsync();

        page.Total.ShouldBe(0);
        transport.Requests.Single().RequestUri!.ToString()
            .ShouldBe("https://bank.sandbox.cybrid.app/api/accounts?bank_guid=bank-guid-1&page=0&per_page=20");
        tokens.RequestedScopes.ShouldBe([CybridScopes.AccountsRead]);
    }

    [Fact]
    public async Task CustomerCreateUsesExecuteScopeAndSnakeCaseBody()
    {
        var (client, transport, tokens) = TestClient.CreateOk("""{"guid":"cust-1","state":"storing","type":"individual"}""");

        var customer = await client.Customers.CreateAsync(new CreateCybridCustomerRequest
        {
            Type = "individual",
            Name = new CybridName { First = "Fulano", Last = "De Tal" },
            EmailAddress = "fulano@example.com",
        });

        customer.Guid.ShouldBe("cust-1");
        customer.State.ShouldBe("storing");

        var request = transport.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Body!.ShouldContain("\"type\":\"individual\"");
        request.Body!.ShouldContain("\"email_address\":\"fulano@example.com\"");
        tokens.RequestedScopes.ShouldBe([CybridScopes.CustomersExecute]);
    }

    [Fact]
    public async Task ExternalBankAccountDeleteUsesExecuteScope()
    {
        var (client, transport, tokens) = TestClient.CreateOk("""{"guid":"eba-1","state":"deleting"}""");

        var deleted = await client.ExternalBankAccounts.DeleteAsync("eba-1");

        deleted.State.ShouldBe("deleting");
        transport.Requests.Single().Method.ShouldBe(HttpMethod.Delete);
        tokens.RequestedScopes.ShouldBe([CybridScopes.ExternalBankAccountsExecute]);
    }

    [Fact]
    public async Task PricesListParsesRootArray()
    {
        var (client, transport, _) = TestClient.CreateOk("""[{"symbol":"USDC-USD","buy_price":100,"sell_price":100}]""");

        var prices = await client.Prices.ListAsync("USDC-USD");

        prices.Count.ShouldBe(1);
        prices[0].Symbol.ShouldBe("USDC-USD");
        transport.Requests.Single().RequestUri!.Query.ShouldContain("symbol=USDC-USD");
    }

    [Fact]
    public async Task NotFoundMapsMessageCodeAndErrorMessage()
    {
        // Forma de erro real validada ao vivo (2026-07-15).
        var (client, _, _) = TestClient.Create(
            (HttpStatusCode.NotFound, """{"status":404,"error_message":"Record not found","message_code":"not_found"}"""));

        var error = await Should.ThrowAsync<CybridApiException>(
            async () => await client.Transfers.GetAsync("t-inexistente"));

        error.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        error.ErrorCode.ShouldBe("not_found");
        error.Message.ShouldContain("Record not found");
    }

    [Fact]
    public async Task UnauthorizedRetriesOnceWithRefreshedTokenThenSucceeds()
    {
        var (client, transport, tokens) = TestClient.Create(
            (HttpStatusCode.Unauthorized, """{"status":401}"""),
            (HttpStatusCode.OK, """{"guid":"t-1","state":"completed"}"""));

        var transfer = await client.Transfers.GetAsync("t-1");

        transfer.State.ShouldBe("completed");
        transport.Requests.Count.ShouldBe(2);

        // 1ª emissão + renovação forçada pelo 401 — ambos com o MESMO scope.
        tokens.RequestedScopes.ShouldBe([CybridScopes.TransfersRead, CybridScopes.TransfersRead]);
    }

    [Fact]
    public async Task CancellationTokenIsHonored()
    {
        var (client, _, _) = TestClient.CreateOk();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(
            async () => await client.Quotes.GetAsync("q-1", cts.Token));
    }

    [Fact]
    public async Task EmptySuccessBodyWhereValueExpectedThrows()
    {
        var (client, _, _) = TestClient.Create((HttpStatusCode.OK, "null"));

        var error = await Should.ThrowAsync<CybridApiException>(
            async () => await client.Trades.GetAsync("t-1"));

        error.Message.ShouldContain("corpo JSON vazio");
    }
}
