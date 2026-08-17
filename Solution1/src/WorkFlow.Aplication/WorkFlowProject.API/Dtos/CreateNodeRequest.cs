using System.Text.Json.Serialization;

namespace WorkFlowProject.API.Dtos;

/// <summary>
/// Dados necessários para criar um novo Node dentro de um Workflow.
/// O campo <see cref="Type"/> determina quais dos demais campos são obrigatórios.
/// </summary>
public class CreateNodeRequest
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do Node: "Sql" ou "Http".
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NodeRequestType Type { get; set; }

    // Campos obrigatórios quando Type = Sql
    public string? ConnectionStringKey { get; set; }
    public string? Table { get; set; }
    public List<string>? Fields { get; set; }

    // Campos obrigatórios quando Type = Http
    public string? Url { get; set; }
    public HttpMethodRequestType? Method { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
}

public enum NodeRequestType
{
    Sql = 1,
    Http = 2
}

public enum HttpMethodRequestType
{
    Get = 1,
    Post = 2,
    Put = 3,
    Delete = 4,
    Patch = 5
}
