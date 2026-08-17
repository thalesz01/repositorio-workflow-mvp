using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Enums;
using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Infraestruction.Data;

namespace WorkFlowProject.Infraestruction.Repositories;

/// <summary>
/// Repositório responsável pela persistência da entidade NodeExecution, utilizando Dapper.
/// </summary>
public class NodeExecutionRepository : BaseRepository, INodeExecutionRepository
{
    public NodeExecutionRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public Task CreateAsync(NodeExecution nodeExecution)
    {
        const string sql = @"INSERT INTO NodeExecutions (Id, WorkflowExecutionId, NodeId, Status, Input, Output, Error, StartedAt, FinishedAt)
                              VALUES (@Id, @WorkflowExecutionId, @NodeId, @Status, @Input, @Output, @Error, @StartedAt, @FinishedAt)";

        return ExecuteAsync(sql, new
        {
            nodeExecution.Id,
            nodeExecution.WorkflowExecutionId,
            nodeExecution.NodeId,
            Status = (int)nodeExecution.Status,
            nodeExecution.Input,
            nodeExecution.Output,
            nodeExecution.Error,
            nodeExecution.StartedAt,
            nodeExecution.FinishedAt
        });
    }

    public Task UpdateAsync(NodeExecution nodeExecution)
    {
        const string sql = @"UPDATE NodeExecutions
                              SET Status = @Status, Output = @Output, Error = @Error, FinishedAt = @FinishedAt
                              WHERE Id = @Id";

        return ExecuteAsync(sql, new
        {
            nodeExecution.Id,
            Status = (int)nodeExecution.Status,
            nodeExecution.Output,
            nodeExecution.Error,
            nodeExecution.FinishedAt
        });
    }

    public async Task<NodeExecution?> GetLastByWorkflowExecutionIdAsync(Guid workflowExecutionId)
    {
        const string sql = @"SELECT TOP 1 Id, WorkflowExecutionId, NodeId, Status, Input, Output, Error, StartedAt, FinishedAt
                              FROM NodeExecutions
                              WHERE WorkflowExecutionId = @WorkflowExecutionId
                              ORDER BY StartedAt DESC";

        var row = await QueryFirstOrDefaultAsync<NodeExecutionRow>(sql, new { WorkflowExecutionId = workflowExecutionId });
        return row is null ? null : MapToEntity(row);
    }

    public async Task<IEnumerable<NodeExecution>> GetByWorkflowExecutionIdAsync(Guid workflowExecutionId)
    {
        const string sql = @"SELECT Id, WorkflowExecutionId, NodeId, Status, Input, Output, Error, StartedAt, FinishedAt
                              FROM NodeExecutions
                              WHERE WorkflowExecutionId = @WorkflowExecutionId
                              ORDER BY StartedAt";

        var rows = await QueryAsync<NodeExecutionRow>(sql, new { WorkflowExecutionId = workflowExecutionId });
        return rows.Select(MapToEntity);
    }

    private static NodeExecution MapToEntity(NodeExecutionRow row) =>
        new(row.Id, row.WorkflowExecutionId, row.NodeId, (ExecutionStatus)row.Status, row.Input, row.Output, row.Error, row.StartedAt, row.FinishedAt);

    /// <summary>
    /// Representa a linha crua lida da tabela NodeExecutions.
    /// </summary>
    private sealed class NodeExecutionRow
    {
        public Guid Id { get; set; }
        public Guid WorkflowExecutionId { get; set; }
        public Guid NodeId { get; set; }
        public int Status { get; set; }
        public string? Input { get; set; }
        public string? Output { get; set; }
        public string? Error { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
