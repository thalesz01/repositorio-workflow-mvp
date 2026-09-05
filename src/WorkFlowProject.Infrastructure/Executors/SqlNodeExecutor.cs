using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Enums;
using WorkFlowProject.Domain.Interfaces.Services;
// Desambigua do Microsoft.Data.SqlClient.SqlCommand, importado acima para o SqlConnection.
using SqlCommand = WorkFlowProject.Domain.Entities.Commands.SqlCommand;

namespace WorkFlowProject.Infrastructure.Executors;

/// <summary>
/// Executor responsável por processar Nodes do tipo SQL, realizando um SELECT simples
/// sobre a tabela e os campos configurados no SqlCommand.
/// </summary>
public class SqlNodeExecutor : INodeExecutor
{
    private readonly IConfiguration _configuration;

    public SqlNodeExecutor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool CanExecute(Node node) => node.Type == NodeType.Sql;

    public async Task<string?> ExecuteAsync(Node node, string? input)
    {
        SqlNode sqlNode = (SqlNode)node;
        SqlCommand command = sqlNode.SqlCommand;

        string? connectionString = _configuration.GetConnectionString(sqlNode.ConnectionStringKey);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (!sqlNode.ConnectionStringKey.Contains('='))
            {
                throw new InvalidOperationException($"Connection string '{sqlNode.ConnectionStringKey}' não foi configurada.");
            }

            connectionString = sqlNode.ConnectionStringKey;
        }

        string fields = string.Join(", ", command.Fields.Select(f => $"[{f}]"));
        string sql = $"SELECT TOP 1 {fields} FROM [{command.Table}]";

        using IDbConnection connection = new SqlConnection(connectionString);
        dynamic? result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql);

        return result is null ? null : JsonSerializer.Serialize((IDictionary<string, object>)result);
    }
}
