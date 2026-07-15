using System.Text.Json;
using System.Text.Json.Serialization;

namespace CambioReal.Cybrid.Serialization;

/// <summary>Convenções de JSON da Cybrid Bank API.</summary>
public static class CybridJson
{
    /// <summary>
    /// Nomes de campo em <c>snake_case</c> (<c>bank_guid</c>, <c>customer_guid</c>,
    /// <c>receive_amount</c>, <c>external_bank_account_guid</c>, …) — confirmado na spec OpenAPI
    /// oficial (<c>bank.sandbox.cybrid.app/api/schema/v1/swagger.yaml</c>, v0.129) e nas respostas
    /// vivas do sandbox (2026-07-15).
    /// </summary>
    /// <remarks>
    /// Sem <see cref="JsonStringEnumConverter"/> global: os valores fechados da Cybrid são
    /// lowercase snake (<c>storage</c>, <c>completed</c>, <c>crypto_transfer</c>, …), mas o
    /// conjunto é aberto e versionado pela plataforma (a spec ganha valores novos a cada release
    /// v0.12x) — campos de valor fechado são <see cref="string"/> simples em <c>Models/</c>, com
    /// os valores conhecidos em classes de constantes. Mesma regra do goal-loop aplicada ao
    /// bs2-sdk/bexs-sdk.
    /// </remarks>
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }
}
