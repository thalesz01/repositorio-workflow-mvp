using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Infraestruction.Data;

namespace WorkFlowProject.Infraestruction.Repositories;

/// <summary>
/// Repositório responsável pela persistência da entidade Workflow, utilizando Dapper.
/// </summary>
public class WorkflowRepository : BaseRepository, IWorkflowRepository
{
    private readonly INodeRepository _nodeRepository;

    public WorkflowRepository(IDbConnectionFactory connectionFactory, INodeRepository nodeRepository)
        : base(connectionFactory)
    {
        _nodeRepository = nodeRepository;
    }

    public async Task<Workflow?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT Id, Name FROM Workflows WHERE Id = @Id";
        var row = await QueryFirstOrDefaultAsync<WorkflowRow>(sql, new { Id = id });

        if (row is null)
        {
            return null;
        }

        var nodes = await _nodeRepository.GetByWorkflowIdAsync(id);
        return new Workflow(row.Id, row.Name, nodes);
    }

    public async Task<IEnumerable<Workflow>> GetAllAsync()
    {
        const string sql = "SELECT Id, Name FROM Workflows";
        var rows = await QueryAsync<WorkflowRow>(sql);
        return rows.Select(r => new Workflow(r.Id, r.Name));
    }

    public Task CreateAsync(Workflow workflow)
    {
        const string sql = "INSERT INTO Workflows (Id, Name) VALUES (@Id, @Name)";
        return ExecuteAsync(sql, new { workflow.Id, workflow.Name });
    }

    /// <summary>
    /// Representa a linha crua lida da tabela Workflows.
    /// </summary>
    private sealed class WorkflowRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
