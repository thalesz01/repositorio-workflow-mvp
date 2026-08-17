using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Enums;

namespace WorkFlowProject.Domain.Interfaces.Services;

/// <summary>
/// Regras de negócio relacionadas à entidade Node.
/// </summary>
public interface INodeService
{
    /// <summary>
    /// Cria e adiciona um Node do tipo Sql ao final da cadeia de execução do Workflow informado.
    /// </summary>
    Task<SqlNode> CreateSqlNodeAsync(Guid workflowId, string name, string connectionStringKey, string table, List<string> fields);

    /// <summary>
    /// Cria e adiciona um Node do tipo Http ao final da cadeia de execução do Workflow informado.
    /// </summary>
    Task<HttpNode> CreateHttpNodeAsync(Guid workflowId, string name, string url, HttpMethodType method, string? body, Dictionary<string, string>? headers);

    /// <summary>
    /// Obtém um Node pelo Id.
    /// Lança <see cref="Exceptions.NodeNotFoundException"/> caso não exista.
    /// </summary>
    Task<Node> GetByIdAsync(Guid nodeId);

    /// <summary>
    /// Obtém todos os Nodes de um Workflow.
    /// </summary>
    Task<IEnumerable<Node>> GetByWorkflowIdAsync(Guid workflowId);
}
