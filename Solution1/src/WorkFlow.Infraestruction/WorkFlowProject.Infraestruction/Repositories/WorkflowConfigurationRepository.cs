using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Infraestruction.Data;

namespace WorkFlowProject.Infraestruction.Repositories;

/// <summary>
/// Repositório responsável pela leitura das configurações associadas a um Workflow.
/// </summary>
public class WorkflowConfigurationRepository : BaseRepository, IWorkflowConfigurationRepository
{
    public WorkflowConfigurationRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<WorkflowConfiguration>> GetByWorkflowIdAsync(Guid workflowId)
    {
        const string sql = @"SELECT WorkflowId, [Key] AS ConfigurationKey, Value
                             FROM WorkflowConfigurations
                             WHERE WorkflowId = @WorkflowId
                             ORDER BY [Key]";

        var rows = await QueryAsync<WorkflowConfigurationRow>(sql, new { WorkflowId = workflowId });
        return rows.Select(row => new WorkflowConfiguration(row.WorkflowId, row.ConfigurationKey, row.Value));
    }

    private sealed class WorkflowConfigurationRow
    {
        public Guid WorkflowId { get; set; }
        public string ConfigurationKey { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
