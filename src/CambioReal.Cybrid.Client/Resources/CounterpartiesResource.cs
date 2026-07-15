using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Contrapartes (beneficiários de payout). <c>counterparties</c>.</summary>
public sealed class CounterpartiesResource
{
    private readonly CybridClient client;

    internal CounterpartiesResource(CybridClient client) => this.client = client;

    /// <summary>Lista counterparties do bank. <c>GET counterparties</c> — validado ao vivo (11 reais).</summary>
    public Task<CybridListPage<CybridCounterparty>> ListAsync(
        int page = 0, int perPage = 20, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridCounterparty>>(
            CybridPaths.List(CybridPaths.Counterparties, client.BankGuid, page, perPage),
            CybridScopes.CounterpartiesRead,
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
