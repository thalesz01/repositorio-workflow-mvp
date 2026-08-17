namespace WorkFlowProject.Domain.Entities;

/// <summary>
/// Representa um processo de Workflow, composto por uma sequência encadeada de Nodes (etapas de execução).
/// </summary>
public class Workflow
{
    private readonly List<Node> _nodes = new();

    /// <summary>
    /// Identificador único do Workflow.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Nome descritivo do Workflow.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Nodes (etapas) que compõem este Workflow, na ordem em que foram adicionados.
    /// </summary>
    public IReadOnlyCollection<Node> Nodes => _nodes.AsReadOnly();

    public Workflow(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    /// <summary>
    /// Construtor utilizado para reidratar um Workflow a partir de dados já persistidos.
    /// </summary>
    public Workflow(Guid id, string name, IEnumerable<Node>? nodes = null)
    {
        Id = id;
        Name = name;

        if (nodes is not null)
        {
            _nodes.AddRange(nodes.OrderBy(n => n.Order));
        }
    }

    /// <summary>
    /// Adiciona um Node ao final da cadeia de execução do Workflow,
    /// encadeando-o automaticamente ao Node anterior (NextNodeId).
    /// </summary>
    public void AddNode(Node node)
    {
        if (_nodes.Count > 0)
        {
            _nodes[^1].SetNextNode(node.Id);
        }

        _nodes.Add(node);
    }
}
