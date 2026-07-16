namespace CambioReal.Cybrid.Models;

/// <summary>
/// Corpo de <c>POST quotes</c> — subconjunto da spec oficial v0.129 usado pelos fluxos da
/// plataforma (trading e funding/crypto_transfer). Valores em inteiros na menor unidade do asset.
/// </summary>
public sealed record CreateCybridQuoteRequest
{
    /// <summary>
    /// Valores da spec: <c>trading</c> (default upstream), <c>funding</c>, <c>crypto_transfer</c>,
    /// <c>inter_account</c>, <c>book_transfer</c>. Constantes em <see cref="CybridProductTypes"/>.
    /// </summary>
    public string? ProductType { get; init; }

    public string? BankGuid { get; init; }
    public string? CustomerGuid { get; init; }

    /// <summary>Exatamente um entre <see cref="ReceiveAmount"/>/<see cref="DeliverAmount"/> por cotação.</summary>
    public long? ReceiveAmount { get; init; }

    public long? DeliverAmount { get; init; }

    /// <summary>Asset (funding/crypto_transfer — ex.: <c>USDC</c>, <c>USD</c>).</summary>
    public string? Asset { get; init; }

    /// <summary>Símbolo (trading — ex.: <c>USDC-USD</c>).</summary>
    public string? Symbol { get; init; }

    /// <summary>Valores da spec: <c>deposit</c>, <c>withdrawal</c>, <c>buy</c>, <c>sell</c>.</summary>
    public string? Side { get; init; }

    /// <summary>Rail de pagamento (funding): <c>ach</c>, <c>eft</c>, <c>wire</c>, <c>rtp</c>, <c>etransfer</c>.</summary>
    public string? PaymentRail { get; init; }

    public string? SourceAccountGuid { get; init; }
    public string? DestinationAccountGuid { get; init; }
    public string? NetworkAddress { get; init; }

    /// <summary>Fees customizadas da cotação (até 2). Opcional para os product_types trading/funding/crypto_transfer/lightning_transfer/trading_exit.</summary>
    public IReadOnlyList<CybridQuoteFee>? Fees { get; init; }

    /// <summary>Contas de destino para transações em lote em blockchains UTXO. Opcional quando <c>product_type: crypto_transfer</c>.</summary>
    public IReadOnlyList<CybridQuoteDestinationAccount>? DestinationAccounts { get; init; }

    /// <summary>Guid do trade relacionado — só presente em trades <c>exit</c>. Obrigatório quando <c>product_type: trading_exit</c>.</summary>
    public string? ReferenceTradeGuid { get; init; }
}

/// <summary>Fee customizada de uma cotação — spec oficial (<c>PostFee</c>).</summary>
public sealed record CybridQuoteFee
{
    /// <summary>Valores da spec: <c>spread</c>, <c>fixed</c>.</summary>
    public required string Type { get; init; }

    /// <summary>Percentual em basis points — obrigatório quando <see cref="Type"/> é <c>spread</c>.</summary>
    public long? SpreadFee { get; init; }

    /// <summary>Valor fixo (fiat) — obrigatório quando <see cref="Type"/> é <c>fixed</c>.</summary>
    public long? FixedFee { get; init; }
}

/// <summary>Entrada de conta de destino para quotes crypto_transfer em lote — spec oficial (<c>PostQuoteEntry</c>).</summary>
public sealed record CybridQuoteDestinationAccount
{
    /// <summary>Único valor da spec: <c>external_wallet</c>.</summary>
    public required string Type { get; init; }

    public required string Guid { get; init; }
    public long? ReceiveAmount { get; init; }
    public long? DeliverAmount { get; init; }
}

/// <summary>Cotação — resposta de <c>POST/GET quotes</c>. Spec oficial v0.129, listagem validada ao vivo.</summary>
public sealed record CybridQuote
{
    public string? Guid { get; init; }
    public string? ProductType { get; init; }
    public string? BankGuid { get; init; }
    public string? CustomerGuid { get; init; }
    public string? Symbol { get; init; }
    public string? Side { get; init; }
    public long? ReceiveAmount { get; init; }
    public long? DeliverAmount { get; init; }
    public long? Fee { get; init; }
    public string? Asset { get; init; }
    public long? NetworkFee { get; init; }
    public string? NetworkFeeAsset { get; init; }
    public string? NetworkAddress { get; init; }
    public string? TradeGuid { get; init; }
    public string? TransferGuid { get; init; }
    public DateTimeOffset? IssuedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>Product types conhecidos de quote/transfer (conjunto aberto, versionado pela spec).</summary>
public static class CybridProductTypes
{
    public const string Trading = "trading";
    public const string Funding = "funding";
    public const string CryptoTransfer = "crypto_transfer";
    public const string InterAccount = "inter_account";
    public const string BookTransfer = "book_transfer";
}

/// <summary>Sides conhecidos.</summary>
public static class CybridQuoteSides
{
    public const string Deposit = "deposit";
    public const string Withdrawal = "withdrawal";
    public const string Buy = "buy";
    public const string Sell = "sell";
}
