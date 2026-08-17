using WorkFlowProject.Domain.Entities;

namespace WorkFlowProject.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato de persistência do histórico de status de uma execução de Workflow.
/// </summary>
public interface IWorkflowExecutionLogRepository
{
    Task CreateAsync(WorkflowExecutionLog log);

    Task<IEnumerable<WorkflowExecutionLog>> GetByWorkflowExecutionIdAsync(Guid workflowExecutionId);
}
