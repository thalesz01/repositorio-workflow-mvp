namespace WorkFlowProject.API.Dtos;

/// <summary>
/// Representa um Node retornado pela API, com os campos aplicáveis conforme o Type (Sql ou Http).
/// </summary>
public class NodeResponse
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? NextNodeId { get; set; }

    // Campos específicos de Sql
    public string? ConnectionStringKey { get; set; }
    public string? Table { get; set; }
    public List<string>? Fields { get; set; }

    // Campos específicos de Http
    public string? Url { get; set; }
    public string? Method { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
}
