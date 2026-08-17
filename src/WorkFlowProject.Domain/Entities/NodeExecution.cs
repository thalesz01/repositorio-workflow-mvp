using WorkFlowProject.Domain.Enums;

namespace WorkFlowProject.Domain.Entities;

/// <summary>
/// Representa o histórico de execução de um Node específico dentro de uma WorkflowExecution,
/// contendo os dados de entrada e saída utilizados na etapa.
/// </summary>
public class NodeExecution
{
    public Guid Id { get; private set; }
    public Guid WorkflowExecutionId { get; private set; }
    public Guid NodeId { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public string? Input { get; private set; }
    public string? Output { get; private set; }
    public string? Error { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }

    public NodeExecution(Guid workflowExecutionId, Guid nodeId, string? input)
    {
        Id = Guid.NewGuid();
        WorkflowExecutionId = workflowExecutionId;
        NodeId = nodeId;
        Input = input;
        Status = ExecutionStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Construtor utilizado para reidratar uma NodeExecution a partir de dados já persistidos.
    /// </summary>
    public NodeExecution(Guid id, Guid workflowExecutionId, Guid nodeId, ExecutionStatus status, string? input, string? output, string? error, DateTime startedAt, DateTime? finishedAt)
    {
        Id = id;
        WorkflowExecutionId = workflowExecutionId;
        NodeId = nodeId;
        Status = status;
        Input = input;
        Output = output;
        Error = error;
        StartedAt = startedAt;
        FinishedAt = finishedAt;
    }

    /// <summary>
    /// Marca a etapa como concluída com sucesso, registrando o resultado produzido.
    /// </summary>
    public void Complete(string? output)
    {
        Output = output;
        Status = ExecutionStatus.Completed;
        FinishedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca a etapa como falha, registrando a mensagem de erro.
    /// </summary>
    public void Fail(string error)
    {
        Error = error;
        Status = ExecutionStatus.Failed;
        FinishedAt = DateTime.UtcNow;
    }
}
