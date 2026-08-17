using WorkFlowProject.Domain.Enums;

namespace WorkFlowProject.Domain.Entities;

/// <summary>
/// Representa uma execução (instância) de um Workflow, controlando em qual Node
/// a execução está atualmente e seu status geral.
/// </summary>
public class WorkflowExecution
{
    public Guid Id { get; private set; }
    public Guid WorkflowId { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public Guid? CurrentNodeId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }

    public WorkflowExecution(Guid workflowId, Guid? firstNodeId)
    {
        Id = Guid.NewGuid();
        WorkflowId = workflowId;
        Status = ExecutionStatus.Pending;
        CurrentNodeId = firstNodeId;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Construtor utilizado para reidratar uma WorkflowExecution a partir de dados já persistidos.
    /// </summary>
    public WorkflowExecution(Guid id, Guid workflowId, ExecutionStatus status, Guid? currentNodeId, DateTime startedAt, DateTime? finishedAt)
    {
        Id = id;
        WorkflowId = workflowId;
        Status = status;
        CurrentNodeId = currentNodeId;
        StartedAt = startedAt;
        FinishedAt = finishedAt;
    }

    /// <summary>
    /// Marca a execução como em andamento.
    /// </summary>
    public void MarkAsRunning() => Status = ExecutionStatus.Running;

    /// <summary>
    /// Avança a execução para o próximo Node da cadeia. Caso não haja próximo Node, marca como concluída.
    /// </summary>
    public void MoveToNextNode(Guid? nextNodeId)
    {
        if (nextNodeId is null)
        {
            Status = ExecutionStatus.Completed;
            FinishedAt = DateTime.UtcNow;
            CurrentNodeId = null;
            return;
        }

        CurrentNodeId = nextNodeId;
        Status = ExecutionStatus.Running;
    }

    /// <summary>
    /// Marca a execução como falha.
    /// </summary>
    public void MarkAsFailed()
    {
        Status = ExecutionStatus.Failed;
        FinishedAt = DateTime.UtcNow;
    }
}
