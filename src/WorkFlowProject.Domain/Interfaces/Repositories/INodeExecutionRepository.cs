using WorkFlowProject.Domain.Entities;

namespace WorkFlowProject.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato de persistência para a entidade NodeExecution.
/// </summary>
public interface INodeExecutionRepository
{
    Task CreateAsync(NodeExecution nodeExecution);

    Task UpdateAsync(NodeExecution nodeExecution);

    /// <summary>
    /// Obtém a última execução de Node registrada para uma WorkflowExecution (usada como Input do próximo Node).
    /// </summary>
    Task<NodeExecution?> GetLastByWorkflowExecutionIdAsync(Guid workflowExecutionId);

    /// <summary>
    /// Obtém todas as execuções de Node de uma WorkflowExecution, em ordem cronológica.
    /// </summary>
    Task<IEnumerable<NodeExecution>> GetByWorkflowExecutionIdAsync(Guid workflowExecutionId);
}
