namespace CambioReal.Cybrid.Models;

/// <summary>Corpo de <c>POST customers</c> — spec oficial (<c>type</c> obrigatório).</summary>
public sealed record CreateCybridCustomerRequest
{
    /// <summary><c>individual</c> ou <c>business</c>.</summary>
    public required string Type { get; init; }

    public CybridName? Name { get; init; }
    public CybridAddress? Address { get; init; }
    public string? DateOfBirth { get; init; }
    public string? PhoneNumber { get; init; }
    public string? EmailAddress { get; init; }
    public IReadOnlyList<CybridIdentificationNumber>? IdentificationNumbers { get; init; }
    public string? Website { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
}

/// <summary>
/// Corpo de <c>PATCH customers/{guid}</c> — spec oficial (<c>PatchCustomer</c>). O ÚNICO campo
/// aceito é <c>state</c>, e o ÚNICO valor permitido é <c>unverified</c> (reseta o customer para o
/// pipeline KYC, forçando nova verificação). NÃO é um update genérico de PII — endereço, telefone
/// e e-mail não são alteráveis por este endpoint apesar do nome sugerir o contrário; confirmado
/// contra o schema oficial <c>PatchCustomer</c> (<c>bank.sandbox.cybrid.app/api/schema/v1/swagger.yaml</c>).
/// </summary>
public sealed record PatchCybridCustomerRequest
{
    /// <summary>Único valor aceito pela spec: <c>unverified</c>. Ver <see cref="CybridCustomerPatchStates"/>.</summary>
    public string? State { get; init; }
}

/// <summary>Valores aceitos por <see cref="PatchCybridCustomerRequest.State"/> (spec oficial: enum fechado de 1 valor).</summary>
public static class CybridCustomerPatchStates
{
    public const string Unverified = "unverified";
}

/// <summary>Customer — spec oficial v0.129, listagem validada ao vivo (208 customers reais).</summary>
public sealed record CybridCustomer
{
    public string? Guid { get; init; }
    public string? BankGuid { get; init; }
    public string? Type { get; init; }

    /// <summary>Valores conhecidos: <c>storing</c>, <c>unverified</c>, <c>verified</c>, <c>rejected</c>, <c>frozen</c> (conjunto aberto).</summary>
    public string? State { get; init; }

    public CybridName? Name { get; init; }
    public CybridAddress? Address { get; init; }
    public string? DateOfBirth { get; init; }
    public string? PhoneNumber { get; init; }
    public string? EmailAddress { get; init; }
    public IReadOnlyList<CybridIdentificationNumber>? IdentificationNumbers { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
