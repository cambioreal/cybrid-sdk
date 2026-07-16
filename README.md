# cybrid-sdk

Cliente .NET tipado (`CambioReal.Cybrid.Client`) para a **Cybrid Bank API** — embedded finance
cripto/fiat: customers/KYC, counterparties, contas externas (Plaid/raw routing), contas de
depósito, quotes, trades e transfers (funding/crypto/book), preços spot. Mesmo padrão do
`cambioreal/kira-sdk`, `ripple-sdk`, `bs2-sdk` e `bexs-sdk`: SDK modela a API nativa do provider,
zero dependência de `CambioReal.Contracts`.

```csharp
services.AddCybridClient(options =>
{
    options.Environment = CybridEnvironment.Sandbox;   // default deliberado
    options.ClientId = configuration["Cybrid:ClientId"]!;
    options.ClientSecret = configuration["Cybrid:ClientSecret"]!;
    options.BankGuid = configuration["Cybrid:BankGuid"]!;
});

var client = provider.GetRequiredService<CybridClient>();

var bank = await client.Banks.GetAsync();
var accounts = await client.Accounts.ListAsync();
var quote = await client.Quotes.CreateAsync(new() { ProductType = "trading", Symbol = "USDC-USD", Side = "buy", ReceiveAmount = 1_000_000, CustomerGuid = guid });
var transfer = await client.Transfers.GetAsync(transferGuid);   // fonte de verdade de status
```

## Autenticação — um token por scope

A Cybrid emite tokens OAuth2 restritos por scope (`customers:execute`, `transfers:read`, …) num
host separado (`id.*.cybrid.app`). O SDK encapsula isso num `ICybridTokenProvider` singleton com
cache POR SCOPE derivado do `expires_in` real (~8h no sandbox), single-flight e 1 retry em 401; o
scope de cada operação viaja por request (`HttpRequestMessage.Options`) — um único pipeline HTTP,
sem um client nomeado por scope.

## O que está confirmado

- **Ao vivo (2026-07-15, SDK real contra sandbox)**: auth por scope, `GET banks/{guid}` (banco
  real "CambioReal Inc KYC"), listagens reais (65 accounts, 93 transfers, 94 quotes, 208
  customers, 131 external bank accounts, 11 counterparties), `GET transfers/{guid}`
  (state `completed`), prices USDC-USD, 404 de domínio (`message_code: not_found`) e criação de
  quote trading (expira sozinha — nada é executado).
- **Spec OpenAPI oficial v0.129.638** como referência de shapes; divergência real capturada:
  `supported_payout_symbols` vem como array de objetos (modelado da realidade).
- **Legado (`cerebro`, read-only)**: fluxos de negócio (onboarding KYC, book transfer com
  participants, funding via external bank account, trading) e o padrão webhook-como-gatilho +
  re-poll (`PayoutNotification`).

## Efeitos e segurança de execução

- Quotes: read-like (expiram sozinhas; a EXECUÇÃO é que move fundos).
- `Trades.CreateAsync`/`Transfers.CreateAsync`: **financial-write** — nunca executados pelos
  testes deste repo; exigem autorização explícita (goal-loop §0.5).
- Nenhuma credencial em código/fixtures; runtime só via `pass cambio-real-v2/cybrid/*`
  (o `bank-guid` do pass era um stub e foi corrigido/validado ao vivo em 2026-07-15).

## Testes

- `tests/CambioReal.Cybrid.Client.Tests` — 51 testes unit/contrato (serialização snake_case,
  token por scope com cache/skew/single-flight, resources com asserção de scope, erros
  `message_code`, retry 401, cancellation). Inclui os 4 gaps P1 (`PATCH customers`,
  `GET identity_verifications` listagem, `PATCH external_bank_accounts`, `PATCH transfers` — os
  três PATCH só como contrato/mock, nunca exercitados) e os filtros de listagem/campos de request
  fechados nos 13 recursos parciais (0.2.0 — ver `CHANGELOG` do PR).
- `tests/CambioReal.Cybrid.Client.SandboxTests` — 8 testes de integração sandbox opt-in, FORA da
  solution. Ver o [README](tests/CambioReal.Cybrid.Client.SandboxTests/README.md) com a última
  execução ao vivo.

## Origem

Discovery completo com matriz de cobertura, fluxos do legado e decisões:
[`docs/providers/cybrid/discovery.md`](docs/providers/cybrid/discovery.md). Consumido pelo
`cybrid-gateway` (contrato canônico `Envelope<T>`/`ProblemDetail` da plataforma).
