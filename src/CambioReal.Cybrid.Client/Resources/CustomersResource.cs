using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Clientes (KYC). <c>customers</c>.</summary>
public sealed class CustomersResource
{
    private readonly CybridClient client;

    internal CustomersResource(CybridClient client) => this.client = client;

    /// <summary>
    /// Lista customers do bank. <c>GET customers</c> — validado ao vivo (208 reais). Filtros
    /// opcionais confirmados na spec oficial (<c>type</c>, <c>guid</c>, <c>label</c>,
    /// <c>include_pii</c> — comma-separated onde aplicável; <c>bank_guid</c> é sempre o
    /// configurado, não exposto). <paramref name="includePii"/> soma o scope
    /// <see cref="CybridScopes.CustomersPiiRead"/> ao token emitido.
    /// </summary>
    public Task<CybridListPage<CybridCustomer>> ListAsync(
        int page = 0, int perPage = 20, string? type = null, string? guid = null, string? label = null,
        bool? includePii = null, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridCustomer>>(
            CybridPaths.List(CybridPaths.Customers, client.BankGuid, page, perPage,
                CybridPaths.Filters(
                    ("type", type),
                    ("guid", guid),
                    ("label", label),
                    ("include_pii", CybridPaths.Bool(includePii)))),
            includePii == true
                ? $"{CybridScopes.CustomersRead} {CybridScopes.CustomersPiiRead}"
                : CybridScopes.CustomersRead,
            cancellationToken);

    /// <summary>Consulta um customer. <c>GET customers/{guid}</c>.</summary>
    public Task<CybridCustomer> GetAsync(string customerGuid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridCustomer>(CybridPaths.Customer(customerGuid), CybridScopes.CustomersRead, cancellationToken);

    /// <summary>Cria um customer. <c>POST customers</c>. Escrita não financeira (entra em pipeline KYC).</summary>
    public Task<CybridCustomer> CreateAsync(CreateCybridCustomerRequest request, CancellationToken cancellationToken = default) =>
        client.PostAsync<CreateCybridCustomerRequest, CybridCustomer>(
            CybridPaths.Customers, request, CybridScopes.CustomersExecute, cancellationToken);

    /// <summary>
    /// Atualiza um customer. <c>PATCH customers/{guid}</c>. Único uso documentado pela spec:
    /// resetar <c>state</c> para <c>unverified</c> (força nova verificação KYC). Escrita não
    /// financeira; scope distinto de <see cref="CybridScopes.CustomersExecute"/>.
    /// </summary>
    public Task<CybridCustomer> PatchAsync(
        string customerGuid, PatchCybridCustomerRequest request, CancellationToken cancellationToken = default) =>
        client.PatchAsync<PatchCybridCustomerRequest, CybridCustomer>(
            CybridPaths.Customer(customerGuid), request, CybridScopes.CustomersWrite, cancellationToken);
}
