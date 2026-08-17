using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Enums;
using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Infraestruction.Data;

namespace WorkFlowProject.Infraestruction.Repositories;

/// <summary>
/// Repositório responsável pelo histórico de status das execuções de Workflow.
/// </summary>
public class WorkflowExecutionLogRepository : BaseRepository, IWorkflowExecutionLogRepository
{
    public WorkflowExecutionLogRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public Task CreateAsync(WorkflowExecutionLog log)
    {
        const string sql = @"INSERT INTO WorkflowExecutionLogs (Id, WorkflowExecutionId, NodeId, Status, Error, CreatedAt)
                             VALUES (@Id, @WorkflowExecutionId, @NodeId, @Status, @Error, @CreatedAt)";

        return ExecuteAsync(sql, new
        {
            log.Id,
            log.WorkflowExecutionId,
            log.NodeId,
            Status = (int)log.Status,
            log.Error,
            log.CreatedAt
        });
    }

    public async Task<IEnumerable<WorkflowExecutionLog>> GetByWorkflowExecutionIdAsync(Guid workflowExecutionId)
    {
        const string sql = @"SELECT Id, WorkflowExecutionId, NodeId, Status, Error, CreatedAt
                             FROM WorkflowExecutionLogs
                             WHERE WorkflowExecutionId = @WorkflowExecutionId
                             ORDER BY CreatedAt";

        var rows = await QueryAsync<WorkflowExecutionLogRow>(sql, new { WorkflowExecutionId = workflowExecutionId });
        return rows.Select(row => new WorkflowExecutionLog(
            row.Id,
            row.WorkflowExecutionId,
            row.NodeId,
            (ExecutionStatus)row.Status,
            row.Error,
            row.CreatedAt));
    }

    private sealed class WorkflowExecutionLogRow
    {
        public Guid Id { get; set; }
        public Guid WorkflowExecutionId { get; set; }
        public Guid? NodeId { get; set; }
        public int Status { get; set; }
        public string? Error { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
