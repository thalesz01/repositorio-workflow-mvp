namespace WorkFlowProject.API.Dtos;

/// <summary>
/// Representa o estado de uma execução de Workflow retornado pela API.
/// </summary>
public class WorkflowExecutionResponse
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? CurrentNodeId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public IEnumerable<WorkflowExecutionLogResponse> Logs { get; set; } = [];
}
