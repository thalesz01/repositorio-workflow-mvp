using WorkFlowProject.Domain.Enums;

namespace WorkFlowProject.Domain.Entities;

/// <summary>
/// Registra uma alteração de status de uma execução de Workflow.
/// </summary>
public class WorkflowExecutionLog
{
    public Guid Id { get; private set; }
    public Guid WorkflowExecutionId { get; private set; }
    public Guid? NodeId { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public string? Error { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public WorkflowExecutionLog(Guid workflowExecutionId, Guid? nodeId, ExecutionStatus status, string? error = null)
    {
        Id = Guid.NewGuid();
        WorkflowExecutionId = workflowExecutionId;
        NodeId = nodeId;
        Status = status;
        Error = error;
        CreatedAt = DateTime.UtcNow;
    }

    public WorkflowExecutionLog(Guid id, Guid workflowExecutionId, Guid? nodeId, ExecutionStatus status, string? error, DateTime createdAt)
    {
        Id = id;
        WorkflowExecutionId = workflowExecutionId;
        NodeId = nodeId;
        Status = status;
        Error = error;
        CreatedAt = createdAt;
    }
}
