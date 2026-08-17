using WorkFlowProject.Domain.Entities;

namespace WorkFlowProject.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato de persistência para a entidade Node.
/// </summary>
public interface INodeRepository
{
    /// <summary>
    /// Obtém um Node pelo seu identificador.
    /// </summary>
    Task<Node?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtém todos os Nodes de um Workflow, ordenados pela sequência de execução.
    /// </summary>
    Task<IEnumerable<Node>> GetByWorkflowIdAsync(Guid workflowId);

    /// <summary>
    /// Cria um novo Node.
    /// </summary>
    Task CreateAsync(Node node);

    /// <summary>
    /// Atualiza o encadeamento (NextNodeId) de um Node existente.
    /// </summary>
    Task UpdateNextNodeAsync(Guid nodeId, Guid nextNodeId);

    /// <summary>
    /// Obtém o último Node (sem próximo) de um Workflow, usado para encadear um novo Node.
    /// </summary>
    Task<Node?> GetLastNodeAsync(Guid workflowId);
}
