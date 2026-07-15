# Cybrid (Cybrid Bank API) — Discovery

Status: fase de descoberta concluída (2026-07-15). Sonda §0.8 do goal-loop **verde e ampla** —
auth + leitura de TODOS os recursos-alvo validados ao vivo com dados reais (sem qualquer bloqueio
estilo BS2/Bexs). SDK em implementação.
Provider order position: **4 of 9** (`GOAL-provider-standalone-sandbox-loop.md`).
Verified: 2026-07-15, contra `pass cambio-real-v2/cybrid/*` no sandbox vivo
(`bank.sandbox.cybrid.app`) + legado `cerebro` (read-only) + **spec OpenAPI oficial v0.129.638**
(`bank.sandbox.cybrid.app/api/schema/v1/swagger.yaml` — 41 paths; guardada como referência).

## 1. Perfil no Provider Protocol

**`Async`** (quote → identity → transfer), plataforma de embedded finance cripto/fiat (rails
USDC/USD do corredor de payout US). Fluxo confirmado no legado: customer (KYC) →
identity_verification → counterparty/external_bank_account → quote → trade (trading) ou transfer
(funding/crypto/book). Como nos demais providers, sem implementação formal de
`IAsyncProviderAdapter` (lacuna conhecida do RFC §6).

## 2. Ambiente e conectividade

| | Sandbox | Produção |
|---|---|---|
| Auth URL | `https://id.sandbox.cybrid.app/` | `https://id.production.cybrid.app/` |
| Base URL | `https://bank.sandbox.cybrid.app/api/` | `https://bank.production.cybrid.app/api/` |
| Credencial | `pass cambio-real-v2/cybrid/{client-id,client-secret,bank-guid}` | não aprovisionada neste loop |
| mTLS / allowlist | Nenhum | — |
| Webhook secret | `cambio-real-v2/cybrid-webhook-secret` (legado consome webhooks — §7) | — |

⚠️ Achados de inventário: (a) credenciais demo hardcoded em `cerebro/config/cybrid.php` (mesmo
padrão S7/S8 de BS2/Bexs — legado é read-only, não corrigir lá); (b) o `bank-guid` no `pass` era
um **stub** (`STUB-CYBRID-BANK-GUID`) — corrigido em 2026-07-15 com o guid real do legado,
validado ao vivo (`GET banks/{guid}` → 200, banco "CambioReal Inc KYC").

## 3. Auth — OAuth2 client_credentials com token POR SCOPE

- `POST {auth}/oauth/token`, host separado (`id.*.cybrid.app`), corpo JSON
  `{grant_type, client_id, client_secret, scope}`.
- **Um token por scope** (`{recurso}:{read|execute}` — ex.: `customers:execute`,
  `transfers:read`); o legado autentica por operação com cache por scope (29min fixos). O SDK usa
  cache por scope derivado do `expires_in` real (**28799s ≈ 8h**, validado ao vivo), single-flight
  por scope, 1 retry em 401. O scope viaja por request via `HttpRequestMessage.Options`
  (decisão de design — evita um `HttpClient` nomeado por scope, diferente do BS2 que tem só 2).
- Validado ao vivo (2026-07-15): token 200 com scopes ecoados; múltiplos scopes numa mesma
  emissão funcionam (`banks:read accounts:read`).

## 4. Validação viva (2026-07-15) — leitura ampla sem bloqueio

| Sonda | Resultado |
|---|---|
| `POST oauth/token` | ✅ 200, Bearer, expires_in=28799 |
| `GET banks/{guid}` | ✅ 200 — "CambioReal Inc KYC", trading symbols USDC/BTC/ETH/... |
| `GET accounts?bank_guid=` | ✅ 200 — **65 contas reais** (trading/fiat) |
| `GET transfers?bank_guid=` | ✅ 200 — **93 transfers reais** |
| `GET quotes?bank_guid=` | ✅ 200 — 94 quotes reais |
| `GET customers?bank_guid=` | ✅ 200 — 208 customers reais |
| `GET trades?bank_guid=` | ✅ 200 — 1 trade |
| `GET external_bank_accounts?bank_guid=` | ✅ 200 — 131 contas externas |
| `GET counterparties?bank_guid=` | ✅ 200 — 11 counterparties |
| `GET prices?symbol=USDC-USD` | ✅ 200 — array na raiz |
| `GET banks/{fictício}` | ✅ 404 de domínio `{"status":404,"error_message":"Record not found","message_code":"not_found"}` |

## 5. Convenções

- **snake_case** em request/response; paginação `{total, page, per_page, objects[]}` (0-based);
  valores monetários **inteiros na menor unidade do asset** (padrão Cybrid — `amount_digits` no
  legado); estados lowercase (`storing`, `completed`, `failed`, …) com conjunto aberto e
  versionado pela spec → DTOs usam `string` + classes de constantes (sem enum global).
- Erro: `{"status", "error_message", "message_code"}` (confirmado ao vivo) — `message_code`
  máquina-legível (`not_found`, etc.); o SDK expõe em `CybridApiException.ErrorCode`.
- Sem header de idempotência documentado (legado não envia; spec não expõe nos POSTs usados).

## 6. Fluxos do legado (fonte: `cerebro/app/Libraries/Cybrid/*`)

- **Onboarding**: `POST customers` (customers:execute) → `POST identity_verifications` (kyc) →
  polling do state/outcome; conta marcada `verified`.
- **Book transfer** (conta local ↔ conta local, `BookService`): quote
  (`product_type: book_transfer`, side `deposit`/`withdrawal`) → `POST transfers`
  (`transfer_type: book`) com `source_account_guid`/`destination_account_guid` +
  `source_participants`/`destination_participants` `[{type: bank|customer, amount, guid}]`.
- **Funding/payout**: external_bank_account (Plaid ou raw routing + counterparty) → quote
  (`funding`) → transfer (`funding`, payment_rail ach/wire).
- **Trading** (USDC↔USD): quote (`trading`, symbol) → `POST trades` (executa a quote).
- **Notificações** (`PayoutNotification`): o legado CONSOME webhooks Cybrid
  (`event_type = {identity_verification|transfer}.{storing|pending|reviewing|completed|failed}`),
  mas SEMPRE re-consulta o recurso (`GET transfers/{guid}`) antes de agir — webhook é gatilho,
  polling é a verdade (mesmo padrão canônico BS2/Bexs/Ripple).

## 7. Webhooks — fora do gateway nesta iteração (decisão registrada)

A Cybrid tem webhooks reais (o legado os consome; secret em
`cambio-real-v2/cybrid-webhook-secret`), mas o gateway desta iteração NÃO expõe endpoint inbound:
o mecanismo confirmado de verdade de status é o GET do recurso; o roteamento de eventos para
sistemas de negócio é responsabilidade do consumidor (hoje o próprio `cerebro`). Incremento
futuro: receiver com verificação de assinatura + re-poll, padrão BS2.

## 8. Matriz de cobertura — recursos no escopo do SDK

Classificação de efeito: cotação = read-like (expira sozinha, não move fundos); trade/transfer
create = **financial-write** (movem saldo); demais creates = non-financial-write.

| # | Endpoint upstream | Métodos | Recurso SDK | Endpoint gateway (planejado `/v1/cybrid/*`) | Efeito | Cleanup | Status sandbox |
|---|---|---|---|---|---|---|---|
| 1 | `oauth/token` (por scope) | POST | `CybridTokenProvider` | interno | read/auth | n/a | ✅ vivo |
| 2 | `banks/{guid}` | GET | `Banks.GetAsync` | `GET /v1/cybrid/bank` | read | n/a | ✅ vivo |
| 3 | `accounts` | GET/POST · GET/{guid} | `Accounts.{List,Get,Create}` | `GET/POST /v1/cybrid/accounts[…]` | read + non-financial-write (create) | sem delete — create só com autorização | ✅ leitura viva |
| 4 | `customers` | GET/POST · GET/{guid} | `Customers.{List,Get,Create}` | idem | read + non-financial-write (pipeline KYC; sem delete) | sem cleanup ⇒ contrato-only por default | ✅ leitura viva |
| 5 | `counterparties` | GET/POST · GET/{guid} | `Counterparties.{List,Get,Create}` | idem | read + non-financial-write (screening; sem delete) | idem | ✅ leitura viva |
| 6 | `identity_verifications` | GET/{guid}/POST | `IdentityVerifications.{Get,Create}` | idem | read + non-financial-write | sem cleanup ⇒ contrato-only | ⚪ leitura por guid depende de recurso existente |
| 7 | `external_bank_accounts` | GET/POST · GET/{guid} · DELETE/{guid} | `ExternalBankAccounts.{List,Get,Create,Delete}` | idem | read + non-financial-write **com cleanup (DELETE)** | ✅ delete existe — ciclo E2E elegível | ✅ leitura viva |
| 8 | `deposit_bank_accounts` | GET/POST · GET/{guid} | `DepositBankAccounts.{List,Get,Create}` | idem | read + non-financial-write | sem delete ⇒ contrato-only | ⚪ |
| 9 | `quotes` | GET/POST · GET/{guid} | `Quotes.{List,Get,Create}` | idem | read + read-like (quote expira sozinha — §0.4) | expira | ✅ leitura viva; create elegível |
| 10 | `trades` | GET/POST · GET/{guid} | `Trades.{List,Get,Create}` | idem | read + **financial-write** (create) | n/a | 🔴 create só com autorização explícita |
| 11 | `transfers` | GET/POST · GET/{guid} | `Transfers.{List,Get,Create}` | idem | read + **financial-write** (create) | n/a | 🔴 create só com autorização explícita |
| 12 | `prices` | GET | `Prices.ListAsync` | `GET /v1/cybrid/prices` | read | n/a | ✅ vivo |
| — | webhook inbound | POST | n/a | **não exposto** (§7) | — | — | ⚪ |

Endpoints da spec fora do escopo (decisão explícita — sem uso no legado/fluxo da plataforma):
`assets`, `symbols`, `deposit_addresses`, `external_wallets`, `files`, `invoices`,
`payment_instructions`, `persona_sessions`, `sardine_sessions`, `plans`, `workflows`,
`executions`, `POST banks`/`PATCH banks`. O SDK mantém o transporte genérico interno; exposição
sob demanda de produto.

## 9. Lacunas, suposições e riscos

1. Spec oficial é versionada com frequência (v0.129.x) — estados/product_types novos aparecem;
   DTOs toleram desconhecidos (strings + ignore de campos extras).
2. `Transfer.source_account`/`destination_account`/`transfer_details`/`hold_details` têm shape
   variável por `transfer_type` — expostos como `JsonElement` cru (sem normalização especulativa).
3. Trades/transfers create nunca exercitados neste loop (financial-write) — contrato/fixture +
   evidência dos 93 transfers/1 trade reais lidos ao vivo.
4. Webhooks fora do gateway (§7) — risco baixo: legado segue consumindo-os diretamente.
5. O quote create com `product_type: book_transfer`/`trading` é seguro em si, mas só será
   exercitado ao vivo se o caso E2E do SandboxTests o exigir com cleanup natural (quote expira).

## 10. Limites de responsabilidade SDK / gateway / plataforma

- **SDK (`cybrid-sdk`)**: modela a Bank API nativa (snake_case, valores em menor unidade), token
  por scope encapsulado, zero dependência de `CambioReal.Contracts`.
- **Gateway (`cybrid-gateway`)**: `/v1/cybrid/*` → `Envelope<T>`/`ProblemDetail` com
  `message_code` upstream preservado; sem autenticação de chamador nesta fase (gap comum); sem
  webhook inbound (§7).
- **Plataforma**: orquestra onboarding/KYC, decide cadência de polling e roteamento de eventos.

## 11. Nenhuma contradição arquitetural encontrada

Cybrid segue o padrão canônico SDK/gateway standalone (perfil `Async`). Decisões locais: token
por scope via request option; subconjunto explícito da spec; webhooks fora do gateway v1.

## 12. Adendo — extração completa do legado (2026-07-15, pós-publicação 0.1.0)

A leitura integral dos 13 arquivos do legado (subagente dedicado) corrigiu/enriqueceu a descoberta:

1. **O fluxo do legado é 100% fiat USD** — nenhum asset cripto aparece nos 13 arquivos (o
   trading USDC visto nos dados vivos do sandbox vem de outra época/uso). Quotes/transfers do
   legado: `funding` (external bank account, ACH) e `book_transfer` (interno bank↔customer);
   NÃO há uso de `trades` no legado.
2. **`accept-version: 2025-10-01`** fixado em toda request do legado — incorporado ao SDK 0.1.1
   (`CybridOptions.ApiVersion`, default igual ao legado).
3. **Identity verification usa também `external_bank_account_guid`** (`{type: bank_account,
   method: account_ownership}`) e `expected_behaviours: ["passed_immediately"]` SÓ em dev —
   campos adicionados ao request no 0.1.1.
4. **EBA raw routing do legado usa `counterparty_bank_account_details`**
   (`{payment_rail, bank_code_type: "ABA", bank_code, account_identifier}`) — nome distinto do
   `counterparty_bank_account` da spec v0.129; ambos expostos no 0.1.1.
5. **Sem retry/idempotência/paginação no legado**; `verify: false` (TLS desabilitado — defeito
   do legado, NÃO replicado; o SDK valida TLS normalmente).
6. **Webhook handler do legado (`PayoutNotification`)**: só `identity_verification.*` e
   `transfer.*`; ignora `storing`; SEMPRE re-consulta o recurso; roteia por `side`; trata
   `failure_code == "refresh_required"` como exigência de re-login Plaid. Confirma a decisão §7
   (webhook = gatilho; GET = verdade).
7. **Fluxo de onboarding completo** (customer → KYC → account fiat → counterparty + watchlists →
   EBA + account_ownership → deposit bank account) documentado na íntegra na extração; o
   `files:read` (download de documentos KYC) segue fora do escopo do SDK (decisão).
