using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Entities.Commands;
using WorkFlowProject.Domain.Enums;
using WorkFlowProject.Domain.Exceptions;
using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Domain.Interfaces.Services;

namespace WorkFlowProject.Domain.Services;

/// <summary>
/// Implementa as regras de negócio relacionadas à entidade Node,
/// incluindo o cálculo da ordem e o encadeamento automático (NextNodeId) na cadeia de execução do Workflow.
/// </summary>
public class NodeService : INodeService
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly INodeRepository _nodeRepository;

    public NodeService(IWorkflowRepository workflowRepository, INodeRepository nodeRepository)
    {
        _workflowRepository = workflowRepository;
        _nodeRepository = nodeRepository;
    }

    public async Task<SqlNode> CreateSqlNodeAsync(Guid workflowId, string name, string connectionStringKey, string table, List<string> fields)
    {
        var (order, lastNode) = await GetNextOrderAndLastNodeAsync(workflowId);

        var node = new SqlNode(workflowId, name, order, connectionStringKey, new SqlCommand(table, fields));
        await _nodeRepository.CreateAsync(node);
        await LinkPreviousNodeAsync(lastNode, node.Id);

        return node;
    }

    public async Task<HttpNode> CreateHttpNodeAsync(Guid workflowId, string name, string url, HttpMethodType method, string? body, Dictionary<string, string>? headers)
    {
        var (order, lastNode) = await GetNextOrderAndLastNodeAsync(workflowId);

        var node = new HttpNode(workflowId, name, order, new HttpCommand(url, method, body, headers));
        await _nodeRepository.CreateAsync(node);
        await LinkPreviousNodeAsync(lastNode, node.Id);

        return node;
    }

    public async Task<Node> GetByIdAsync(Guid nodeId)
    {
        var node = await _nodeRepository.GetByIdAsync(nodeId);
        return node ?? throw new NodeNotFoundException(nodeId);
    }

    public Task<IEnumerable<Node>> GetByWorkflowIdAsync(Guid workflowId) => _nodeRepository.GetByWorkflowIdAsync(workflowId);

    /// <summary>
    /// Garante que o Workflow existe, obtém o último Node cadastrado (se houver) e calcula a próxima ordem de execução.
    /// </summary>
    private async Task<(int Order, Node? LastNode)> GetNextOrderAndLastNodeAsync(Guid workflowId)
    {
        var workflow = await _workflowRepository.GetByIdAsync(workflowId);
        if (workflow is null)
        {
            throw new WorkflowNotFoundException(workflowId);
        }

        var lastNode = await _nodeRepository.GetLastNodeAsync(workflowId);
        var order = (lastNode?.Order ?? -1) + 1;

        return (order, lastNode);
    }

    /// <summary>
    /// Encadeia o Node anterior ao novo Node criado, atualizando seu NextNodeId.
    /// </summary>
    private Task LinkPreviousNodeAsync(Node? lastNode, Guid newNodeId)
    {
        return lastNode is not null
            ? _nodeRepository.UpdateNextNodeAsync(lastNode.Id, newNodeId)
            : Task.CompletedTask;
    }
}
