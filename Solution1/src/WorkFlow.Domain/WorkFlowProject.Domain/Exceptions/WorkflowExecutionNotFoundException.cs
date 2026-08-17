namespace WorkFlowProject.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma WorkflowExecution não é encontrada.
/// </summary>
public class WorkflowExecutionNotFoundException : Exception
{
    public WorkflowExecutionNotFoundException(Guid executionId)
        : base($"Execução de Workflow '{executionId}' não encontrada.")
    {
    }
}
