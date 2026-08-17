namespace WorkFlowProject.Domain.Enums;

/// <summary>
/// Representa o status de execução de um Workflow ou de um Node dentro de uma execução.
/// </summary>
public enum ExecutionStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4
}
