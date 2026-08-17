using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Enums;
using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Infraestruction.Data;

namespace WorkFlowProject.Infraestruction.Repositories;

/// <summary>
/// Repositório responsável pela persistência da entidade WorkflowExecution, utilizando Dapper.
/// </summary>
public class WorkflowExecutionRepository : BaseRepository, IWorkflowExecutionRepository
{
    public WorkflowExecutionRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<WorkflowExecution?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT Id, WorkflowId, Status, CurrentNodeId, StartedAt, FinishedAt FROM WorkflowExecutions WHERE Id = @Id";
        var row = await QueryFirstOrDefaultAsync<WorkflowExecutionRow>(sql, new { Id = id });
        return row is null ? null : MapToEntity(row);
    }

    public async Task<IEnumerable<WorkflowExecution>> GetPendingExecutionsAsync()
    {
        const string sql = @"SELECT Id, WorkflowId, Status, CurrentNodeId, StartedAt, FinishedAt
                              FROM WorkflowExecutions
                              WHERE Status IN (@Pending, @Running)
                              ORDER BY StartedAt";

        var rows = await QueryAsync<WorkflowExecutionRow>(sql, new
        {
            Pending = (int)ExecutionStatus.Pending,
            Running = (int)ExecutionStatus.Running
        });

        return rows.Select(MapToEntity);
    }

    public Task CreateAsync(WorkflowExecution execution)
    {
        const string sql = @"INSERT INTO WorkflowExecutions (Id, WorkflowId, Status, CurrentNodeId, StartedAt, FinishedAt)
                              VALUES (@Id, @WorkflowId, @Status, @CurrentNodeId, @StartedAt, @FinishedAt)";

        return ExecuteAsync(sql, new
        {
            execution.Id,
            execution.WorkflowId,
            Status = (int)execution.Status,
            execution.CurrentNodeId,
            execution.StartedAt,
            execution.FinishedAt
        });
    }

    public Task UpdateAsync(WorkflowExecution execution)
    {
        const string sql = @"UPDATE WorkflowExecutions
                              SET Status = @Status, CurrentNodeId = @CurrentNodeId, FinishedAt = @FinishedAt
                              WHERE Id = @Id";

        return ExecuteAsync(sql, new
        {
            execution.Id,
            Status = (int)execution.Status,
            execution.CurrentNodeId,
            execution.FinishedAt
        });
    }

    private static WorkflowExecution MapToEntity(WorkflowExecutionRow row) =>
        new(row.Id, row.WorkflowId, (ExecutionStatus)row.Status, row.CurrentNodeId, row.StartedAt, row.FinishedAt);

    /// <summary>
    /// Representa a linha crua lida da tabela WorkflowExecutions.
    /// </summary>
    private sealed class WorkflowExecutionRow
    {
        public Guid Id { get; set; }
        public Guid WorkflowId { get; set; }
        public int Status { get; set; }
        public Guid? CurrentNodeId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
