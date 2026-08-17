namespace WorkFlowProject.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando um Workflow não é encontrado.
/// </summary>
public class WorkflowNotFoundException : Exception
{
    public WorkflowNotFoundException(Guid workflowId)
        : base($"Workflow '{workflowId}' não encontrado.")
    {
    }
}
