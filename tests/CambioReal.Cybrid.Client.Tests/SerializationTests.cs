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

    [Fact]
    public void PatchCustomerRequestSerializesStateOnly()
    {
        var request = new PatchCybridCustomerRequest { State = CybridCustomerPatchStates.Unverified };

        var json = JsonSerializer.Serialize(request, CybridJson.Options);

        json.ShouldBe("""{"state":"unverified"}""");
    }

    [Fact]
    public void PatchExternalBankAccountRequestSerializesStateOnly()
    {
        var request = new PatchCybridExternalBankAccountRequest { State = CybridExternalBankAccountPatchStates.RefreshRequired };

        var json = JsonSerializer.Serialize(request, CybridJson.Options);

        json.ShouldBe("""{"state":"refresh_required"}""");
    }

    [Fact]
    public void PatchTransferRequestSerializesParticipantsWithRequiredGuid()
    {
        var request = new PatchCybridTransferRequest
        {
            SourceParticipants = [new PatchCybridTransferParticipant { Type = "bank", Amount = 2500, Guid = "bank-1" }],
        };

        var json = JsonSerializer.Serialize(request, CybridJson.Options);

        json.ShouldContain("\"source_participants\":[{\"type\":\"bank\",\"amount\":2500,\"guid\":\"bank-1\"}]");
        json.ShouldNotContain("destination_participants");
    }

    [Fact]
    public void CreateExternalBankAccountRequestSerializesPlaidProcessorFields()
    {
        var request = new CreateCybridExternalBankAccountRequest
        {
            Name = "Conta Plaid",
            AccountKind = "plaid_processor_token",
            PlaidProcessorToken = "proc-token-1",
            PlaidInstitutionId = "ins_1",
            PlaidAccountMask = "0000",
            PlaidAccountName = "Checking",
        };

        var json = JsonSerializer.Serialize(request, CybridJson.Options);

        json.ShouldContain("\"plaid_institution_id\":\"ins_1\"");
        json.ShouldContain("\"plaid_account_mask\":\"0000\"");
        json.ShouldContain("\"plaid_account_name\":\"Checking\"");
    }

    [Fact]
    public void CreateIdentityVerificationRequestSerializesKybFields()
    {
        var request = new CreateCybridIdentityVerificationRequest
        {
            Type = "kyc",
            Method = "attested_business_registration",
            RequireTaxId = true,
            Aliases = [new CybridAlias { Full = "Doing Business As" }],
            Website = "https://example.com",
            NatureOfBusiness = "Software",
            DirectorCustomerGuids = ["dir-1"],
            UltimateBeneficialOwners = [new CybridUltimateBeneficialOwner { CustomerGuid = "ubo-1", OwnershipPercentage = 33.5m }],
            SupportingFileGuids = ["file-1"],
            RegisteredAddress = new CybridAddress { CountryCode = "US" },
            BusinessIndustry = "Crypto / Digital Assets / Blockchain",
            BusinessFundsSource = "Funds from individual customers",
            BusinessFundsDestination = "To vendors or suppliers",
            Occupation = "Engineer",
            BiometricsVerified = true,
        };

        var json = JsonSerializer.Serialize(request, CybridJson.Options);

        json.ShouldContain("\"require_tax_id\":true");
        json.ShouldContain("\"aliases\":[{\"full\":\"Doing Business As\"}]");
        json.ShouldContain("\"nature_of_business\":\"Software\"");
        json.ShouldContain("\"director_customer_guids\":[\"dir-1\"]");
        json.ShouldContain("\"ultimate_beneficial_owners\":[{\"customer_guid\":\"ubo-1\",\"ownership_percentage\":33.5}]");
        json.ShouldContain("\"supporting_file_guids\":[\"file-1\"]");
        json.ShouldContain("\"registered_address\":{\"country_code\":\"US\"}");
        json.ShouldContain("\"business_industry\":\"Crypto / Digital Assets / Blockchain\"");
        json.ShouldContain("\"business_funds_source\":\"Funds from individual customers\"");
        json.ShouldContain("\"business_funds_destination\":\"To vendors or suppliers\"");
        json.ShouldContain("\"occupation\":\"Engineer\"");
        json.ShouldContain("\"biometrics_verified\":true");
    }

    [Fact]
    public void CreateQuoteRequestSerializesFeesDestinationAccountsAndReferenceTrade()
    {
        var request = new CreateCybridQuoteRequest
        {
            ProductType = CybridProductTypes.Trading,
            Symbol = "USDC-USD",
            Side = CybridQuoteSides.Buy,
            ReceiveAmount = 1_000_000,
            Fees = [new CybridQuoteFee { Type = "spread", SpreadFee = 50 }],
            DestinationAccounts = [new CybridQuoteDestinationAccount { Type = "external_wallet", Guid = "wallet-1", ReceiveAmount = 1_000_000 }],
            ReferenceTradeGuid = "trade-1",
        };

        var json = JsonSerializer.Serialize(request, CybridJson.Options);

        json.ShouldContain("\"fees\":[{\"type\":\"spread\",\"spread_fee\":50}]");
        json.ShouldContain("\"destination_accounts\":[{\"type\":\"external_wallet\",\"guid\":\"wallet-1\",\"receive_amount\":1000000}]");
        json.ShouldContain("\"reference_trade_guid\":\"trade-1\"");
    }

    [Fact]
    public void CreateTransferRequestSerializesFundingAndSandboxFields()
    {
        var request = new CreateCybridTransferRequest
        {
            QuoteGuid = "quote-1",
            TransferType = "instant_funding",
            SendAsDepositBankAccountGuid = "dba-1",
            BankFiatAccountGuid = "bank-fiat-1",
            CustomerFiatAccountGuid = "cust-fiat-1",
            NetworkFeeAccountGuid = "fee-acct-1",
            ExpectedBehaviours = ["force_review"],
        };

        var json = JsonSerializer.Serialize(request, CybridJson.Options);

        json.ShouldContain("\"send_as_deposit_bank_account_guid\":\"dba-1\"");
        json.ShouldContain("\"bank_fiat_account_guid\":\"bank-fiat-1\"");
        json.ShouldContain("\"customer_fiat_account_guid\":\"cust-fiat-1\"");
        json.ShouldContain("\"network_fee_account_guid\":\"fee-acct-1\"");
        json.ShouldContain("\"expected_behaviours\":[\"force_review\"]");
    }

    [Fact]
    public void IdentityVerificationListDeserializesFromLiveShape()
    {
        const string json = """
        {"total":2,"page":0,"per_page":20,"objects":[
            {"guid":"iv-1","type":"kyc","method":"watchlists","state":"completed","outcome":"passed"},
            {"guid":"iv-2","type":"bank_account","method":"account_ownership","state":"waiting"}]}
        """;

        var page = JsonSerializer.Deserialize<CybridListPage<CybridIdentityVerification>>(json, CybridJson.Options)!;

        page.Total.ShouldBe(2);
        page.Objects[0].Outcome.ShouldBe("passed");
        page.Objects[1].State.ShouldBe("waiting");
    }
}
