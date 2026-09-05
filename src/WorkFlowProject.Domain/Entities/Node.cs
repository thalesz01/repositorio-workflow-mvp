using WorkFlowProject.Domain.Entities.Commands;
using WorkFlowProject.Domain.Enums;

namespace WorkFlowProject.Domain.Entities;

/// <summary>
/// Representa uma etapa (node) de execução dentro de um Workflow.
/// Cada Node possui um identificador único e pode estar encadeado a um próximo Node,
/// formando a sequência de execução do processo.
/// </summary>
public abstract class Node
{
    /// <summary>
    /// Identificador único do Node.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identificador do Workflow ao qual este Node pertence.
    /// </summary>
    public Guid WorkflowId { get; private set; }

    /// <summary>
    /// Identificador do próximo Node a ser executado após este.
    /// Nulo quando este for o último Node da cadeia.
    /// </summary>
    public Guid? NextNodeId { get; private set; }

    /// <summary>
    /// Posição/ordem do Node dentro do Workflow.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Nome descritivo do Node.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Tipo de execução representado por este Node (Sql ou Http).
    /// </summary>
    public NodeType Type { get; }

    /// <summary>
    /// Comando com as informações do que deve ser executado nesta etapa.
    /// Para Nodes SQL, contém a tabela e os campos envolvidos; para Nodes HTTP,
    /// contém as informações da requisição a ser realizada.
    /// </summary>
    public NodeCommand Command { get; private set; }

    protected Node(Guid workflowId, string name, int order, NodeType type, NodeCommand command)
    {
        Id = Guid.NewGuid();
        WorkflowId = workflowId;
        Name = name;
        Order = order;
        Type = type;
        Command = command;
    }

    /// <summary>
    /// Construtor utilizado para reidratar um Node a partir de dados já persistidos
    /// (ex: ao carregar do banco de dados), preservando o Id e o encadeamento originais.
    /// </summary>
    protected Node(Guid id, Guid workflowId, string name, int order, NodeType type, NodeCommand command, Guid? nextNodeId)
    {
        Id = id;
        WorkflowId = workflowId;
        Name = name;
        Order = order;
        Type = type;
        Command = command;
        NextNodeId = nextNodeId;
    }

    /// <summary>
    /// Define o próximo Node a ser executado após este, criando o encadeamento da cadeia de execução.
    /// </summary>
    public void SetNextNode(Guid nextNodeId)
    {
        NextNodeId = nextNodeId;
    }
}
