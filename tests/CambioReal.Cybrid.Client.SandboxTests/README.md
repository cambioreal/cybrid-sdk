# CambioReal.Cybrid.Client.SandboxTests

Testes de integração sandbox real, opt-in — nunca incluído em `Cybrid.slnx`, nunca rodado por
padrão em CI (goal-loop §2.5).

## Rodar

```bash
export CYBRID_SANDBOX_CLIENT_ID="$(pass show cambio-real-v2/cybrid/client-id)"
export CYBRID_SANDBOX_CLIENT_SECRET="$(pass show cambio-real-v2/cybrid/client-secret)"
export CYBRID_SANDBOX_BANK_GUID="$(pass show cambio-real-v2/cybrid/bank-guid)"
dotnet test tests/CambioReal.Cybrid.Client.SandboxTests/CambioReal.Cybrid.Client.SandboxTests.csproj
unset CYBRID_SANDBOX_CLIENT_ID CYBRID_SANDBOX_CLIENT_SECRET CYBRID_SANDBOX_BANK_GUID
```

Sem as variáveis, os testes falham explicitamente — não há skip silencioso.

Criação de QUOTE (read-like — expira sozinha, §0.4) exige `CYBRID_SANDBOX_ALLOW_WRITE=1`.
**Criação de trade/transfer (financial-write) não existe neste projeto por design** (goal §0.5).

## Última execução ao vivo — 2026-07-15 (com `CYBRID_SANDBOX_ALLOW_WRITE=1`)

```
Passed QuoteCreateLiveExpiresAlone [3 s]
  POST quotes: criado, product=trading USDC-USD, expira sozinho — nenhum trade/transfer executado.
Passed AccountsAndTransfersListLive [3 s]
  GET accounts: 200, total=65. GET transfers: 200, total=93. GET transfers/{guid}: 200, state=completed.
Passed AuthenticatesLiveAgainstSandbox [1 s]
  sandbox: token issued (banks:read), length=1295 (masked).
Passed FictitiousGuidReturnsDomainNotFound [1 s]
  GET accounts/{fictício}: 404, message_code=not_found.
Passed PricesReadLive [1 s]
  GET prices: 200 (USDC-USD).
Passed BankReadsLive
  GET banks/{guid}: 200, "CambioReal Inc KYC".

Passed: 6, Failed: 0, Total: 6
```

Achado capturado por estes testes: `supported_payout_symbols` no bank vem como array de OBJETOS
(`{symbol, country_code, participants_type, route}`) na resposta viva — a spec v0.129 declara
array de string. Modelado a partir da realidade (`CybridPayoutSymbol`).
