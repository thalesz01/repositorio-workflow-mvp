namespace WorkFlowProject.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando um Node não é encontrado.
/// </summary>
public class NodeNotFoundException : Exception
{
    public NodeNotFoundException(Guid nodeId)
        : base($"Node '{nodeId}' não encontrado.")
    {
    }
}
