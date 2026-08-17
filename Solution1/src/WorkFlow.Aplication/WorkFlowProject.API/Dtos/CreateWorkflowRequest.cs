namespace WorkFlowProject.API.Dtos;

/// <summary>
/// Dados necessários para criar um novo Workflow.
/// </summary>
public class CreateWorkflowRequest
{
    public string Name { get; set; } = string.Empty;
}
