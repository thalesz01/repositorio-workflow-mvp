namespace WorkFlowProject.API.Dtos;

/// <summary>
/// Representa uma alteração de status de uma execução de Workflow.
/// </summary>
public class WorkflowExecutionLogResponse
{
    public Guid Id { get; set; }
    public Guid? NodeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
}
