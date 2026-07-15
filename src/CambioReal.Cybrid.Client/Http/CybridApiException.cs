using System.Net;

namespace CambioReal.Cybrid.Http;

/// <summary>Erro devolvido pela Cybrid Bank API.</summary>
public class CybridApiException : Exception
{
    /// <summary>Cria uma exceção sem contexto de resposta.</summary>
    public CybridApiException()
    {
    }

    /// <summary>Cria uma exceção com mensagem.</summary>
    public CybridApiException(string message)
        : base(message)
    {
    }

    /// <summary>Cria uma exceção com mensagem e causa.</summary>
    public CybridApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Cria uma exceção a partir de uma resposta da API.</summary>
    public CybridApiException(HttpStatusCode statusCode, string? errorCode, string message, string? responseBody)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ResponseBody = responseBody;
    }

    /// <summary>Status HTTP da resposta.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// <c>message_code</c> devolvido pela Cybrid (<c>not_found</c>, <c>invalid_parameter</c>, …) —
    /// forma de erro confirmada ao vivo:
    /// <c>{"status":404,"error_message":"Record not found","message_code":"not_found"}</c>.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>Corpo bruto da resposta, para diagnóstico.</summary>
    public string? ResponseBody { get; }
}

/// <summary>A autenticação falhou mesmo após uma renovação de token.</summary>
public sealed class CybridAuthenticationException : CybridApiException
{
    /// <inheritdoc/>
    public CybridAuthenticationException()
    {
    }

    /// <inheritdoc/>
    public CybridAuthenticationException(string message)
        : base(message)
    {
    }

    /// <inheritdoc/>
    public CybridAuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc/>
    public CybridAuthenticationException(HttpStatusCode statusCode, string? errorCode, string message, string? responseBody)
        : base(statusCode, errorCode, message, responseBody)
    {
    }
}
