using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Clientes (KYC). <c>customers</c>.</summary>
public sealed class CustomersResource
{
    private readonly CybridClient client;

    internal CustomersResource(CybridClient client) => this.client = client;

    /// <summary>Lista customers do bank. <c>GET customers</c> — validado ao vivo (208 reais).</summary>
    public Task<CybridListPage<CybridCustomer>> ListAsync(
        int page = 0, int perPage = 20, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridCustomer>>(
            CybridPaths.List(CybridPaths.Customers, client.BankGuid, page, perPage),
            CybridScopes.CustomersRead,
            cancellationToken);

    /// <summary>Consulta um customer. <c>GET customers/{guid}</c>.</summary>
    public Task<CybridCustomer> GetAsync(string customerGuid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridCustomer>(CybridPaths.Customer(customerGuid), CybridScopes.CustomersRead, cancellationToken);

    /// <summary>Cria um customer. <c>POST customers</c>. Escrita não financeira (entra em pipeline KYC).</summary>
    public Task<CybridCustomer> CreateAsync(CreateCybridCustomerRequest request, CancellationToken cancellationToken = default) =>
        client.PostAsync<CreateCybridCustomerRequest, CybridCustomer>(
            CybridPaths.Customers, request, CybridScopes.CustomersExecute, cancellationToken);
}
