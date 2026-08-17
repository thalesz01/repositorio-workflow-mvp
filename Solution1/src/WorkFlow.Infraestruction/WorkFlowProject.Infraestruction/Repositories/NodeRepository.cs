using System.Text.Json;
using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Entities.Commands;
using WorkFlowProject.Domain.Enums;
using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Infraestruction.Data;

namespace WorkFlowProject.Infraestruction.Repositories;

/// <summary>
/// Repositório responsável pela persistência da entidade Node, utilizando Dapper.
/// O comando de execução (SqlCommand ou HttpCommand) é persistido serializado em JSON.
/// </summary>
public class NodeRepository : BaseRepository, INodeRepository
{
    public NodeRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<Node?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT Id, WorkflowId, Name, [Order], Type, NextNodeId, ConnectionStringKey, CommandJson FROM Nodes WHERE Id = @Id";
        var row = await QueryFirstOrDefaultAsync<NodeRow>(sql, new { Id = id });
        return row is null ? null : MapToEntity(row);
    }

    public async Task<IEnumerable<Node>> GetByWorkflowIdAsync(Guid workflowId)
    {
        const string sql = "SELECT Id, WorkflowId, Name, [Order], Type, NextNodeId, ConnectionStringKey, CommandJson FROM Nodes WHERE WorkflowId = @WorkflowId ORDER BY [Order]";
        var rows = await QueryAsync<NodeRow>(sql, new { WorkflowId = workflowId });
        return rows.Select(MapToEntity);
    }

    public Task CreateAsync(Node node)
    {
        const string sql = @"INSERT INTO Nodes (Id, WorkflowId, Name, [Order], Type, NextNodeId, ConnectionStringKey, CommandJson)
                              VALUES (@Id, @WorkflowId, @Name, @Order, @Type, @NextNodeId, @ConnectionStringKey, @CommandJson)";

        var (connectionStringKey, commandJson) = ExtractPersistenceData(node);

        return ExecuteAsync(sql, new
        {
            node.Id,
            node.WorkflowId,
            node.Name,
            node.Order,
            Type = (int)node.Type,
            node.NextNodeId,
            ConnectionStringKey = connectionStringKey,
            CommandJson = commandJson
        });
    }

    public Task UpdateNextNodeAsync(Guid nodeId, Guid nextNodeId)
    {
        const string sql = "UPDATE Nodes SET NextNodeId = @NextNodeId WHERE Id = @Id";
        return ExecuteAsync(sql, new { Id = nodeId, NextNodeId = nextNodeId });
    }

    public async Task<Node?> GetLastNodeAsync(Guid workflowId)
    {
        const string sql = @"SELECT TOP 1 Id, WorkflowId, Name, [Order], Type, NextNodeId, ConnectionStringKey, CommandJson
                              FROM Nodes WHERE WorkflowId = @WorkflowId ORDER BY [Order] DESC";
        var row = await QueryFirstOrDefaultAsync<NodeRow>(sql, new { WorkflowId = workflowId });
        return row is null ? null : MapToEntity(row);
    }

    private static (string? connectionStringKey, string commandJson) ExtractPersistenceData(Node node)
    {
        return node switch
        {
            SqlNode sqlNode => (sqlNode.ConnectionStringKey, JsonSerializer.Serialize(sqlNode.SqlCommand)),
            HttpNode httpNode => (null, JsonSerializer.Serialize(httpNode.HttpCommand)),
            _ => throw new InvalidOperationException($"Tipo de Node não suportado: {node.GetType().Name}")
        };
    }

    private static Node MapToEntity(NodeRow row)
    {
        var type = (NodeType)row.Type;

        return type switch
        {
            NodeType.Sql => new SqlNode(
                row.Id,
                row.WorkflowId,
                row.Name,
                row.Order,
                row.NextNodeId,
                row.ConnectionStringKey ?? throw new InvalidOperationException("ConnectionStringKey não pode ser nulo para um SqlNode."),
                JsonSerializer.Deserialize<SqlCommand>(row.CommandJson) ?? throw new InvalidOperationException("Falha ao desserializar SqlCommand.")),

            NodeType.Http => new HttpNode(
                row.Id,
                row.WorkflowId,
                row.Name,
                row.Order,
                row.NextNodeId,
                JsonSerializer.Deserialize<HttpCommand>(row.CommandJson) ?? throw new InvalidOperationException("Falha ao desserializar HttpCommand.")),

            _ => throw new InvalidOperationException($"Tipo de Node desconhecido: {row.Type}")
        };
    }

    /// <summary>
    /// Representa a linha crua lida da tabela Nodes.
    /// </summary>
    private sealed class NodeRow
    {
        public Guid Id { get; set; }
        public Guid WorkflowId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public int Type { get; set; }
        public Guid? NextNodeId { get; set; }
        public string? ConnectionStringKey { get; set; }
        public string CommandJson { get; set; } = string.Empty;
    }
}
