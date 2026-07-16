using CambioReal.Cybrid.Auth;
using CambioReal.Cybrid.Models;

namespace CambioReal.Cybrid.Resources;

/// <summary>Transferências (funding/crypto/book). <c>transfers</c>.</summary>
public sealed class TransfersResource
{
    private readonly CybridClient client;

    internal TransfersResource(CybridClient client) => this.client = client;

    /// <summary>
    /// Lista transfers do bank. <c>GET transfers</c> — validado ao vivo (93 reais). Filtros
    /// opcionais confirmados na spec oficial (<c>guid</c>, <c>transfer_type</c>,
    /// <c>customer_guid</c>, <c>account_guid</c>, <c>state</c>, <c>side</c>, <c>label</c>,
    /// <c>txn_hash</c>, faixas <c>created_at_gte/lt</c> e <c>updated_at_gte/lt</c> em ISO8601;
    /// <c>bank_guid</c> é sempre o configurado, não exposto).
    /// </summary>
    public Task<CybridListPage<CybridTransfer>> ListAsync(
        int page = 0, int perPage = 20, string? guid = null, string? transferType = null,
        string? customerGuid = null, string? accountGuid = null, string? state = null, string? side = null,
        string? label = null, string? txnHash = null, DateTimeOffset? createdAtGte = null,
        DateTimeOffset? createdAtLt = null, DateTimeOffset? updatedAtGte = null, DateTimeOffset? updatedAtLt = null,
        CancellationToken cancellationToken = default) =>
        client.GetAsync<CybridListPage<CybridTransfer>>(
            CybridPaths.List(CybridPaths.Transfers, client.BankGuid, page, perPage,
                CybridPaths.Filters(
                    ("guid", guid),
                    ("transfer_type", transferType),
                    ("customer_guid", customerGuid),
                    ("account_guid", accountGuid),
                    ("state", state),
                    ("side", side),
                    ("label", label),
                    ("txn_hash", txnHash),
                    ("created_at_gte", CybridPaths.Iso(createdAtGte)),
                    ("created_at_lt", CybridPaths.Iso(createdAtLt)),
                    ("updated_at_gte", CybridPaths.Iso(updatedAtGte)),
                    ("updated_at_lt", CybridPaths.Iso(updatedAtLt)))),
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

    /// <summary>
    /// Atualiza uma transfer. <c>PATCH transfers/{guid}</c>. Único uso documentado pela spec:
    /// (re)definir os participantes de uma transfer em andamento. **Sensível** — ajusta uma
    /// transferência financeira já criada; scope distinto de
    /// <see cref="CybridScopes.TransfersExecute"/>. NUNCA exercitado contra sandbox (só
    /// contrato/mock — mesma política de <see cref="CreateAsync"/>).
    /// </summary>
    public Task<CybridTransfer> PatchAsync(
        string transferGuid, PatchCybridTransferRequest request, CancellationToken cancellationToken = default) =>
        client.PatchAsync<PatchCybridTransferRequest, CybridTransfer>(
            CybridPaths.Transfer(transferGuid), request, CybridScopes.TransfersWrite, cancellationToken);
}
