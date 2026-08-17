using WorkFlowProject.Domain.Entities;

namespace WorkFlowProject.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato de leitura das configurações associadas a um Workflow.
/// </summary>
public interface IWorkflowConfigurationRepository
{
    Task<IEnumerable<WorkflowConfiguration>> GetByWorkflowIdAsync(Guid workflowId);
}
