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

    // --- Gaps P1 (4 reais) -------------------------------------------------------------------

    [Fact]
    public async Task CustomerPatchUsesWriteScopeAndSendsStateBody()
    {
        var (client, transport, tokens) = TestClient.CreateOk("""{"guid":"cust-1","state":"unverified"}""");

        var customer = await client.Customers.PatchAsync(
            "cust-1", new PatchCybridCustomerRequest { State = CybridCustomerPatchStates.Unverified });

        customer.State.ShouldBe("unverified");
        var request = transport.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Patch);
        request.RequestUri!.ToString().ShouldBe("https://bank.sandbox.cybrid.app/api/customers/cust-1");
        request.Body!.ShouldBe("""{"state":"unverified"}""");
        tokens.RequestedScopes.ShouldBe([CybridScopes.CustomersWrite]);
    }

    [Fact]
    public async Task IdentityVerificationsListInjectsBankGuidPaginationAndFilters()
    {
        var (client, transport, tokens) = TestClient.CreateOk("""{"total":0,"page":0,"per_page":20,"objects":[]}""");

        await client.IdentityVerifications.ListAsync(customerGuid: "cust-1", state: "completed");

        transport.Requests.Single().RequestUri!.ToString().ShouldBe(
            "https://bank.sandbox.cybrid.app/api/identity_verifications?bank_guid=bank-guid-1&page=0&per_page=20"
            + "&customer_guid=cust-1&state=completed");
        tokens.RequestedScopes.ShouldBe([CybridScopes.IdentityVerificationsRead]);
    }

    [Fact]
    public async Task ExternalBankAccountPatchUsesWriteScopeAndSendsStateBody()
    {
        var (client, transport, tokens) = TestClient.CreateOk("""{"guid":"eba-1","state":"completed"}""");

        var account = await client.ExternalBankAccounts.PatchAsync(
            "eba-1", new PatchCybridExternalBankAccountRequest { State = CybridExternalBankAccountPatchStates.Completed });

        account.State.ShouldBe("completed");
        var request = transport.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Patch);
        request.Body!.ShouldBe("""{"state":"completed"}""");
        tokens.RequestedScopes.ShouldBe([CybridScopes.ExternalBankAccountsWrite]);
    }

    /// <summary>
    /// PATCH transfers é sensível (atualiza uma transferência financeira) — este teste é
    /// contrato/mock puro (nunca contra sandbox real), confirmando shape e scope apenas.
    /// </summary>
    [Fact]
    public async Task TransferPatchUsesWriteScopeAndSendsParticipantsBody()
    {
        var (client, transport, tokens) = TestClient.CreateOk("""{"guid":"t-1","state":"pending"}""");

        var transfer = await client.Transfers.PatchAsync("t-1", new PatchCybridTransferRequest
        {
            DestinationParticipants =
            [
                new PatchCybridTransferParticipant { Type = "customer", Amount = 500, Guid = "cust-1" },
            ],
        });

        transfer.State.ShouldBe("pending");
        var request = transport.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Patch);
        request.Body!.ShouldBe(
            """{"destination_participants":[{"type":"customer","amount":500,"guid":"cust-1"}]}""");
        tokens.RequestedScopes.ShouldBe([CybridScopes.TransfersWrite]);
    }

    // --- Parciais (list/get filtros) ----------------------------------------------------------

    [Fact]
    public async Task AccountsListWithFiltersBuildsQuery()
    {
        var (client, transport, _) = TestClient.CreateOk("""{"total":0,"page":0,"per_page":20,"objects":[]}""");

        await client.Accounts.ListAsync(guid: "a-1", type: "trading", owner: "customer", label: "vip");

        transport.Requests.Single().RequestUri!.ToString().ShouldBe(
            "https://bank.sandbox.cybrid.app/api/accounts?bank_guid=bank-guid-1&page=0&per_page=20"
            + "&guid=a-1&type=trading&owner=customer&label=vip");
    }

    [Fact]
    public async Task CustomersListWithIncludePiiUsesDualScope()
    {
        var (client, transport, tokens) = TestClient.CreateOk("""{"total":0,"page":0,"per_page":20,"objects":[]}""");

        await client.Customers.ListAsync(includePii: true, type: "individual");

        transport.Requests.Single().RequestUri!.ToString().ShouldBe(
            "https://bank.sandbox.cybrid.app/api/customers?bank_guid=bank-guid-1&page=0&per_page=20"
            + "&type=individual&include_pii=true");
        tokens.RequestedScopes.ShouldBe([$"{CybridScopes.CustomersRead} {CybridScopes.CustomersPiiRead}"]);
    }

    [Fact]
    public async Task CounterpartiesListWithFiltersBuildsQuery()
    {
        var (client, transport, _) = TestClient.CreateOk("""{"total":0,"page":0,"per_page":20,"objects":[]}""");

        await client.Counterparties.ListAsync(type: "business", customerGuid: "cust-1");

        transport.Requests.Single().RequestUri!.ToString().ShouldBe(
            "https://bank.sandbox.cybrid.app/api/counterparties?bank_guid=bank-guid-1&page=0&per_page=20"
            + "&type=business&customer_guid=cust-1");
    }

    [Fact]
    public async Task DepositBankAccountsListWithFiltersBuildsQuery()
    {
        var (client, transport, _) = TestClient.CreateOk("""{"total":0,"page":0,"per_page":20,"objects":[]}""");

        await client.DepositBankAccounts.ListAsync(state: "created", type: "fbo");

        transport.Requests.Single().RequestUri!.ToString().ShouldBe(
            "https://bank.sandbox.cybrid.app/api/deposit_bank_accounts?bank_guid=bank-guid-1&page=0&per_page=20"
            + "&type=fbo&state=created");
    }

    [Fact]
    public async Task ExternalBankAccountsListWithFiltersBuildsQuery()
    {
        var (client, transport, _) = TestClient.CreateOk("""{"total":0,"page":0,"per_page":20,"objects":[]}""");

        await client.ExternalBankAccounts.ListAsync(asset: "USD", state: "completed");

        transport.Requests.Single().RequestUri!.ToString().ShouldBe(
            "https://bank.sandbox.cybrid.app/api/external_bank_accounts?bank_guid=bank-guid-1&page=0&per_page=20"
            + "&asset=USD&state=completed");
    }

    [Fact]
    public async Task ExternalBankAccountGetWithOptionsBuildsQueryAndDualScopeWhenPiiRequested()
    {
        var (client, transport, tokens) = TestClient.CreateOk("""{"guid":"eba-1","state":"completed"}""");

        await client.ExternalBankAccounts.GetAsync(
            "eba-1", forceBalanceRefresh: true, includeBalances: true, includePii: true);

        transport.Requests.Single().RequestUri!.ToString().ShouldBe(
            "https://bank.sandbox.cybrid.app/api/external_bank_accounts/eba-1"
            + "?force_balance_refresh=true&include_balances=true&include_pii=true");
        tokens.RequestedScopes.ShouldBe(
            [$"{CybridScopes.ExternalBankAccountsRead} {CybridScopes.ExternalBankAccountsPiiRead}"]);
    }

    [Fact]
    public async Task GetWithBalancesAsyncDelegatesToFlexibleOverloadWithAllTrue()
    {
        var (client, transport, tokens) = TestClient.CreateOk("""{"guid":"eba-1","state":"completed"}""");

        await client.ExternalBankAccounts.GetWithBalancesAsync("eba-1");

        transport.Requests.Single().RequestUri!.Query.ShouldBe(
            "?force_balance_refresh=true&include_balances=true&include_pii=true");
        tokens.RequestedScopes.ShouldBe(
            [$"{CybridScopes.ExternalBankAccountsRead} {CybridScopes.ExternalBankAccountsPiiRead}"]);
    }

    [Fact]
    public async Task QuotesListWithFiltersBuildsQuery()
    {
        var (client, transport, _) = TestClient.CreateOk("""{"total":0,"page":0,"per_page":20,"objects":[]}""");

        await client.Quotes.ListAsync(productType: CybridProductTypes.Trading, side: CybridQuoteSides.Buy);

        transport.Requests.Single().RequestUri!.ToString().ShouldBe(
            "https://bank.sandbox.cybrid.app/api/quotes?bank_guid=bank-guid-1&page=0&per_page=20"
            + "&product_type=trading&side=buy");
    }

    [Fact]
    public async Task TradesListWithFiltersIncludingDateRangesBuildsQuery()
    {
        var (client, transport, _) = TestClient.CreateOk("""{"total":0,"page":0,"per_page":20,"objects":[]}""");

        var createdAtGte = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await client.Trades.ListAsync(state: "completed", createdAtGte: createdAtGte);

        transport.Requests.Single().RequestUri!.ToString().ShouldBe(
            "https://bank.sandbox.cybrid.app/api/trades?bank_guid=bank-guid-1&page=0&per_page=20"
            + $"&state=completed&created_at_gte={Uri.EscapeDataString(createdAtGte.ToString("O"))}");
    }

    [Fact]
    public async Task TransfersListWithFiltersIncludingDateRangesBuildsQuery()
    {
        var (client, transport, _) = TestClient.CreateOk("""{"total":0,"page":0,"per_page":20,"objects":[]}""");

        var updatedAtLt = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);
        await client.Transfers.ListAsync(transferType: "funding", updatedAtLt: updatedAtLt);

        transport.Requests.Single().RequestUri!.ToString().ShouldBe(
            "https://bank.sandbox.cybrid.app/api/transfers?bank_guid=bank-guid-1&page=0&per_page=20"
            + $"&transfer_type=funding&updated_at_lt={Uri.EscapeDataString(updatedAtLt.ToString("O"))}");
    }
}
