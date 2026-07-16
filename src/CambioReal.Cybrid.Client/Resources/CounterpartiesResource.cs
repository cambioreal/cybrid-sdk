using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Contrapartes (beneficiários de payout). <c>counterparties</c>.</summary>
public sealed class CounterpartiesResource
{
    private readonly CybridClient client;

    internal CounterpartiesResource(CybridClient client) => this.client = client;

    /// <summary>
    /// Lista counterparties do bank. <c>GET counterparties</c> — validado ao vivo (11 reais).
    /// Filtros opcionais confirmados na spec oficial (<c>type</c>, <c>customer_guid</c>,
    /// <c>guid</c>, <c>label</c>, <c>include_pii</c>; <c>bank_guid</c> é sempre o configurado, não
    /// exposto). <paramref name="includePii"/> soma o scope
    /// <see cref="CybridScopes.CounterpartiesPiiRead"/> ao token emitido.
    /// </summary>
    public Task<CybridListPage<CybridCounterparty>> ListAsync(
        int page = 0, int perPage = 20, string? type = null, string? customerGuid = null, string? guid = null,
        string? label = null, bool? includePii = null, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridCounterparty>>(
            CybridPaths.List(CybridPaths.Counterparties, client.BankGuid, page, perPage,
                CybridPaths.Filters(
                    ("type", type),
                    ("customer_guid", customerGuid),
                    ("guid", guid),
                    ("label", label),
                    ("include_pii", CybridPaths.Bool(includePii)))),
            includePii == true
                ? $"{CybridScopes.CounterpartiesRead} {CybridScopes.CounterpartiesPiiRead}"
                : CybridScopes.CounterpartiesRead,
            cancellationToken);

    /// <summary>Consulta uma counterparty. <c>GET counterparties/{guid}</c>.</summary>
    public Task<CybridCounterparty> GetAsync(string counterpartyGuid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridCounterparty>(
            CybridPaths.Counterparty(counterpartyGuid), CybridScopes.CounterpartiesRead, cancellationToken);

    /// <summary>Cria uma counterparty. <c>POST counterparties</c>. Escrita não financeira (compliance screening).</summary>
    public Task<CybridCounterparty> CreateAsync(CreateCybridCounterpartyRequest request, CancellationToken cancellationToken = default) =>
        client.PostAsync<CreateCybridCounterpartyRequest, CybridCounterparty>(
            CybridPaths.Counterparties, request, CybridScopes.CounterpartiesExecute, cancellationToken);
}
