using WorkFlowProject.Domain.Entities;

namespace WorkFlowProject.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato de persistência para a entidade Workflow.
/// </summary>
public interface IWorkflowRepository
{
    /// <summary>
    /// Obtém um Workflow pelo seu identificador, incluindo seus Nodes.
    /// </summary>
    Task<Workflow?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtém todos os Workflows cadastrados (sem os Nodes).
    /// </summary>
    Task<IEnumerable<Workflow>> GetAllAsync();

    /// <summary>
    /// Cria um novo Workflow.
    /// </summary>
    Task CreateAsync(Workflow workflow);
}
