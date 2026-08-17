using WorkFlowProject.Domain.Entities.Commands;
using WorkFlowProject.Domain.Enums;

namespace WorkFlowProject.Domain.Entities;

/// <summary>
/// Node responsável por executar uma requisição HTTP como etapa do Workflow.
/// </summary>
public class HttpNode : Node
{
    /// <summary>
    /// Comando HTTP a ser executado, contendo url, método, headers e body da requisição.
    /// </summary>
    public HttpCommand HttpCommand => (HttpCommand)Command;

    public HttpNode(Guid workflowId, string name, int order, HttpCommand command)
        : base(workflowId, name, order, NodeType.Http, command)
    {
    }

    /// <summary>
    /// Construtor utilizado para reidratar um HttpNode a partir de dados já persistidos.
    /// </summary>
    public HttpNode(Guid id, Guid workflowId, string name, int order, Guid? nextNodeId, HttpCommand command)
        : base(id, workflowId, name, order, NodeType.Http, command, nextNodeId)
    {
    }
}

