using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Transferências (funding/crypto/book). <c>transfers</c>.</summary>
public sealed class TransfersResource
{
    private readonly CybridClient client;

    internal TransfersResource(CybridClient client) => this.client = client;

    /// <summary>Lista transfers do bank. <c>GET transfers</c> — validado ao vivo (93 reais).</summary>
    public Task<CybridListPage<CybridTransfer>> ListAsync(
        int page = 0, int perPage = 20, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridTransfer>>(
            CybridPaths.List(CybridPaths.Transfers, client.BankGuid, page, perPage),
            CybridScopes.TransfersRead,
            cancellationToken);

    /// <summary>
    /// Consulta uma transfer. <c>GET transfers/{guid}</c> — única fonte de verdade de status
    /// confirmada (o legado poll-a via cron; webhooks Cybrid existem mas não são a verdade).
    /// </summary>
    public Task<CybridTransfer> GetAsync(string transferGuid, CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridTransfer>(CybridPaths.Transfer(transferGuid), CybridScopes.TransfersRead, cancellationToken);

    /// <summary>
    /// Executa uma cotação de transferência. <c>POST transfers</c>. **Financial-write** — move
    /// fundos (funding ACH/wire, crypto on-chain ou book transfer); não executar contra sandbox
    /// sem autorização explícita (goal-loop §0.5).
    /// </summary>
    public Task<CybridTransfer> CreateAsync(CreateCybridTransferRequest request, CancellationToken cancellationToken = default) =>
        client.PostAsync<CreateCybridTransferRequest, CybridTransfer>(
            CybridPaths.Transfers, request, CybridScopes.TransfersExecute, cancellationToken);
}
