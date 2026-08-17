using WorkFlowProject.Domain.Entities.Commands;
using WorkFlowProject.Domain.Enums;

namespace WorkFlowProject.Domain.Entities;

/// <summary>
/// Node responsável por executar um comando ou consulta SQL como etapa do Workflow.
/// </summary>
public class SqlNode : Node
{
    /// <summary>
    /// Chave de configuração da connection string a ser utilizada na execução (ex: "DefaultConnection").
    /// </summary>
    public string ConnectionStringKey { get; private set; }

    /// <summary>
    /// Comando SQL a ser executado, contendo a tabela e os campos envolvidos.
    /// </summary>
    public SqlCommand SqlCommand => (SqlCommand)Command;

    public SqlNode(Guid workflowId, string name, int order, string connectionStringKey, SqlCommand command)
        : base(workflowId, name, order, NodeType.Sql, command)
    {
        ConnectionStringKey = connectionStringKey;
    }

    /// <summary>
    /// Construtor utilizado para reidratar um SqlNode a partir de dados já persistidos.
    /// </summary>
    public SqlNode(Guid id, Guid workflowId, string name, int order, Guid? nextNodeId, string connectionStringKey, SqlCommand command)
        : base(id, workflowId, name, order, NodeType.Sql, command, nextNodeId)
    {
        ConnectionStringKey = connectionStringKey;
    }
}

