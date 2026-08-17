using WorkFlowProject.Domain.Entities;

namespace WorkFlowProject.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato de persistência para a entidade WorkflowExecution.
/// </summary>
public interface IWorkflowExecutionRepository
{
    Task<WorkflowExecution?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtém as execuções que ainda precisam ser processadas (Pending ou Running).
    /// </summary>
    Task<IEnumerable<WorkflowExecution>> GetPendingExecutionsAsync();

    Task CreateAsync(WorkflowExecution execution);

    Task UpdateAsync(WorkflowExecution execution);
}
