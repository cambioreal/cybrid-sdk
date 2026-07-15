using CambioReal.Cybrid.Tests.Fakes;
using Shouldly;
using Xunit;

namespace CambioReal.Cybrid.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public void ValidOptionsPassValidation()
        => Should.NotThrow(() => TestClient.NewOptions().Validate());

    [Theory]
    [InlineData("", "secret-1", "bank-1")]
    [InlineData("client-1", "", "bank-1")]
    [InlineData("client-1", "secret-1", "")]
    public void MissingRequiredFieldThrows(string clientId, string clientSecret, string bankGuid)
    {
        var options = TestClient.NewOptions();
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.BankGuid = bankGuid;

        Should.Throw<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void BaseAddressWithoutTrailingSlashThrows()
    {
        var options = TestClient.NewOptions();
        options.BaseAddress = new Uri("https://bank.sandbox.cybrid.app/api", UriKind.Absolute);

        Should.Throw<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void SandboxResolvesToSandboxHosts()
    {
        CybridEnvironment.Sandbox.GetBaseAddress().ToString().ShouldBe("https://bank.sandbox.cybrid.app/api/");
        CybridEnvironment.Sandbox.GetAuthBaseAddress().ToString().ShouldBe("https://id.sandbox.cybrid.app/");
    }

    [Fact]
    public void ProductionResolvesToProductionHosts()
    {
        CybridEnvironment.Production.GetBaseAddress().ToString().ShouldBe("https://bank.production.cybrid.app/api/");
        CybridEnvironment.Production.GetAuthBaseAddress().ToString().ShouldBe("https://id.production.cybrid.app/");
    }
}
