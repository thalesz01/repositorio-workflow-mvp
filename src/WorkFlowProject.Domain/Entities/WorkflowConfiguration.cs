namespace WorkFlowProject.Domain.Entities;

/// <summary>
/// Representa uma configuração identificada por chave para um Workflow.
/// </summary>
public class WorkflowConfiguration
{
    public Guid WorkflowId { get; private set; }
    public string Key { get; private set; }
    public string Value { get; private set; }

    public WorkflowConfiguration(Guid workflowId, string key, string value)
    {
        WorkflowId = workflowId;
        Key = key;
        Value = value;
    }
}
