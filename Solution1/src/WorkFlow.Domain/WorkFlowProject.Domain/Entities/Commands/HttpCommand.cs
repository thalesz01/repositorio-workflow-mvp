using WorkFlowProject.Domain.Enums;

namespace WorkFlowProject.Domain.Entities.Commands;

/// <summary>
/// Comando de execução HTTP, contendo as informações necessárias para realizar a requisição.
/// </summary>
public class HttpCommand : NodeCommand
{
    /// <summary>
    /// URL de destino da requisição HTTP.
    /// </summary>
    public string Url { get; private set; }

    /// <summary>
    /// Método HTTP utilizado na requisição.
    /// </summary>
    public HttpMethodType Method { get; private set; }

    /// <summary>
    /// Cabeçalhos da requisição HTTP.
    /// </summary>
    public Dictionary<string, string> Headers { get; private set; }

    /// <summary>
    /// Corpo da requisição HTTP, quando aplicável (ex: POST, PUT, PATCH).
    /// </summary>
    public string? Body { get; private set; }

    public HttpCommand(string url, HttpMethodType method, string? body = null, Dictionary<string, string>? headers = null)
    {
        Url = url;
        Method = method;
        Body = body;
        Headers = headers ?? new Dictionary<string, string>();
    }
}
