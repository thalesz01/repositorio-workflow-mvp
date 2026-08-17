namespace WorkFlowProject.API.Dtos;

/// <summary>
/// Representa um Workflow retornado pela API.
/// </summary>
public class WorkflowResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<NodeResponse> Nodes { get; set; } = new();
}
