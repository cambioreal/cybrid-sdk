namespace CambioReal.Cybrid;

/// <summary>Configuração do <see cref="CybridClient"/>.</summary>
public sealed class CybridOptions
{
    /// <summary>Nome da seção de configuração sugerida.</summary>
    public const string SectionName = "Cybrid";

    /// <summary>Client ID OAuth2 (client_credentials), gerado no dashboard Cybrid.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Client secret OAuth2. Deve vir do <c>pass</c> ou de um secret store — nunca do código.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// GUID do bank Cybrid alvo (<c>bank_guid</c>) — obrigatório em praticamente toda listagem e
    /// criação. Origem: <c>pass cambio-real-v2/cybrid/bank-guid</c> (corrigido em 2026-07-15 — o
    /// valor era um stub; o guid real foi validado ao vivo contra <c>GET api/banks/{guid}</c>).
    /// </summary>
    public string BankGuid { get; set; } = string.Empty;

    /// <summary>Ambiente alvo. O padrão é <see cref="CybridEnvironment.Sandbox"/>, deliberadamente.</summary>
    public CybridEnvironment Environment { get; set; } = CybridEnvironment.Sandbox;

    /// <summary>Sobrescreve o endereço base da Bank API. Precisa terminar em <c>/</c>.</summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>Sobrescreve o endereço da API de autenticação. Precisa terminar em <c>/</c>.</summary>
    public Uri? AuthBaseAddress { get; set; }

    /// <summary>
    /// Margem de segurança para renovar cada token antes do vencimento real. O sandbox emite
    /// tokens com <c>expires_in</c> de ~8h (28799s, validado ao vivo); o legado cacheava 29min
    /// fixos — aqui a expiração deriva do <c>expires_in</c> real menos esta margem, por scope.
    /// </summary>
    public TimeSpan TokenExpirationSkew { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>Timeout de cada requisição HTTP.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Versão da API pinada via header <c>accept-version</c> em toda requisição — o legado fixa
    /// <c>2025-10-01</c> (achado da extração completa do legado, 2026-07-15). Vazio = não enviar
    /// (a Cybrid serve a versão default). Default mantém o pin do legado, deliberadamente.
    /// </summary>
    public string ApiVersion { get; set; } = "2025-10-01";

    /// <summary>Endereço base efetivo da Bank API.</summary>
    public Uri ResolveBaseAddress() => BaseAddress ?? Environment.GetBaseAddress();

    /// <summary>Endereço efetivo da API de autenticação.</summary>
    public Uri ResolveAuthBaseAddress() => AuthBaseAddress ?? Environment.GetAuthBaseAddress();

    /// <summary>Valida a configuração e lança se estiver inconsistente.</summary>
    /// <exception cref="InvalidOperationException">Alguma credencial obrigatória está ausente ou um base address é inválido.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
        {
            throw new InvalidOperationException($"{nameof(CybridOptions)}.{nameof(ClientId)} é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(ClientSecret))
        {
            throw new InvalidOperationException($"{nameof(CybridOptions)}.{nameof(ClientSecret)} é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(BankGuid))
        {
            throw new InvalidOperationException($"{nameof(CybridOptions)}.{nameof(BankGuid)} é obrigatório.");
        }

        ValidateBaseAddress(ResolveBaseAddress(), nameof(BaseAddress));
        ValidateBaseAddress(ResolveAuthBaseAddress(), nameof(AuthBaseAddress));

        if (TokenExpirationSkew < TimeSpan.Zero)
        {
            throw new InvalidOperationException($"{nameof(TokenExpirationSkew)} não pode ser negativo.");
        }

        if (Timeout <= TimeSpan.Zero)
        {
            throw new InvalidOperationException($"{nameof(Timeout)} precisa ser positivo.");
        }
    }

    private static void ValidateBaseAddress(Uri baseAddress, string propertyName)
    {
        if (!baseAddress.IsAbsoluteUri)
        {
            throw new InvalidOperationException($"{propertyName} precisa ser absoluto.");
        }

        if (!baseAddress.AbsolutePath.EndsWith('/'))
        {
            throw new InvalidOperationException($"{propertyName} precisa terminar em '/' (recebido: '{baseAddress}').");
        }
    }
}
