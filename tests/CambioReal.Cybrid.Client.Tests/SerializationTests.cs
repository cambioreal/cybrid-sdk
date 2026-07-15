using System.Text.Json;
using CambioReal.Cybrid.Models;
using CambioReal.Cybrid.Serialization;
using Shouldly;
using Xunit;

namespace CambioReal.Cybrid.Tests;

public sealed class SerializationTests
{
    [Fact]
    public void CreateQuoteRequestSerializesInSnakeCaseOmittingNulls()
    {
        var request = new CreateCybridQuoteRequest
        {
            ProductType = CybridProductTypes.BookTransfer,
            BankGuid = "bank-1",
            CustomerGuid = "cust-1",
            DeliverAmount = 10_00,
            Asset = "USD",
            Side = CybridQuoteSides.Deposit,
        };

        var json = JsonSerializer.Serialize(request, CybridJson.Options);

        json.ShouldContain("\"product_type\":\"book_transfer\"");
        json.ShouldContain("\"bank_guid\":\"bank-1\"");
        json.ShouldContain("\"customer_guid\":\"cust-1\"");
        json.ShouldContain("\"deliver_amount\":1000");
        json.ShouldContain("\"side\":\"deposit\"");
        json.ShouldNotContain("receive_amount");
        json.ShouldNotContain("symbol");
    }

    [Fact]
    public void CreateTransferRequestSerializesParticipants()
    {
        var request = new CreateCybridTransferRequest
        {
            QuoteGuid = "quote-1",
            TransferType = "book",
            SourceAccountGuid = "acc-src",
            DestinationAccountGuid = "acc-dst",
            SourceParticipants = [new CybridTransferParticipant { Type = "bank", Amount = 1000, Guid = "bank-1" }],
            DestinationParticipants = [new CybridTransferParticipant { Type = "customer", Amount = 1000, Guid = "cust-1" }],
        };

        var json = JsonSerializer.Serialize(request, CybridJson.Options);

        json.ShouldContain("\"quote_guid\":\"quote-1\"");
        json.ShouldContain("\"transfer_type\":\"book\"");
        json.ShouldContain("\"source_participants\":[{\"type\":\"bank\",\"amount\":1000,\"guid\":\"bank-1\"}]");
        json.ShouldContain("\"destination_participants\":[{\"type\":\"customer\",\"amount\":1000,\"guid\":\"cust-1\"}]");
    }

    [Fact]
    public void ListPageDeserializesFromLiveShape()
    {
        // Shape real validado ao vivo (2026-07-15).
        const string json = """
        {"total":65,"page":0,"per_page":2,"objects":[
            {"guid":"8b17db00aaaa","type":"trading","asset":"USDC","state":"created","platform_balance":123456},
            {"guid":"cc7bae35bbbb","type":"trading","asset":"USDC","state":"created"}]}
        """;

        var page = JsonSerializer.Deserialize<CybridListPage<CybridAccount>>(json, CybridJson.Options)!;

        page.Total.ShouldBe(65);
        page.Objects.Count.ShouldBe(2);
        page.Objects[0].Guid.ShouldBe("8b17db00aaaa");
        page.Objects[0].Type.ShouldBe("trading");
        page.Objects[0].PlatformBalance.ShouldBe(123456);
    }

    [Fact]
    public void TransferDeserializesLooseObjectsAsRawJson()
    {
        const string json = """
        {"guid":"t-1","transfer_type":"book","state":"completed","amount":1000,
         "source_account":{"guid":"a-1","type":"fiat"},
         "destination_account":{"guid":"a-2","type":"fiat"},
         "source_participants":[{"type":"bank","amount":1000,"guid":"b-1"}]}
        """;

        var transfer = JsonSerializer.Deserialize<CybridTransfer>(json, CybridJson.Options)!;

        transfer.State.ShouldBe("completed");
        transfer.SourceAccount!.Value.GetProperty("guid").GetString().ShouldBe("a-1");
        transfer.SourceParticipants![0].Type.ShouldBe("bank");
    }

    [Fact]
    public void BankDeserializesFromLiveShape()
    {
        const string json = """
        {"guid":"c37323a2","organization_guid":"39dd59c3","name":"CambioReal Inc KYC",
         "supported_trading_symbols":["USDC-USD","BTC-USD"],"created_at":"2023-09-27T17:27:53.056603Z"}
        """;

        var bank = JsonSerializer.Deserialize<CybridBank>(json, CybridJson.Options)!;

        bank.Name.ShouldBe("CambioReal Inc KYC");
        bank.SupportedTradingSymbols!.Count.ShouldBe(2);
        bank.CreatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UnknownStateValuesSurviveDeserialization()
    {
        // Estados são conjunto aberto versionado pela spec — valor novo não pode quebrar o parse.
        const string json = """{"guid":"t-1","state":"some_future_state"}""";

        var trade = JsonSerializer.Deserialize<CybridTrade>(json, CybridJson.Options)!;

        trade.State.ShouldBe("some_future_state");
    }
}
