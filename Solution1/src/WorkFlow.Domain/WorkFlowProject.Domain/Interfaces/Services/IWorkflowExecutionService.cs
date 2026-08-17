using WorkFlowProject.Domain.Entities;

namespace WorkFlowProject.Domain.Interfaces.Services;

/// <summary>
/// Orquestra a execução de um Workflow, etapa a etapa, através das suas WorkflowExecutions.
/// </summary>
public interface IWorkflowExecutionService
{
    /// <summary>
    /// Inicia uma nova execução para o Workflow informado, deixando-a pronta (Pending) para ser processada.
    /// </summary>
    Task<WorkflowExecution> StartExecutionAsync(Guid workflowId);

    /// <summary>
    /// Consulta uma execução pelo Id.
    /// </summary>
    Task<WorkflowExecution> GetExecutionAsync(Guid executionId);

    /// <summary>
    /// Obtém o histórico de status de uma execução pelo Id.
    /// </summary>
    Task<IEnumerable<WorkflowExecutionLog>> GetExecutionLogsAsync(Guid executionId);

    /// <summary>
    /// Obtém todas as execuções que ainda precisam ser processadas (Pending ou Running).
    /// </summary>
    Task<IEnumerable<WorkflowExecution>> GetPendingExecutionsAsync();

    /// <summary>
    /// Executa todos os Nodes restantes de uma WorkflowExecution, seguindo a cadeia definida por NextNodeId.
    /// </summary>
    Task ExecuteNextStepAsync(Guid executionId);
}
