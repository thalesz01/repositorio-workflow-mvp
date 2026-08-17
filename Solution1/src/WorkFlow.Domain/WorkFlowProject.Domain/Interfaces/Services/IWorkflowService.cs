using WorkFlowProject.Domain.Entities;

namespace WorkFlowProject.Domain.Interfaces.Services;

/// <summary>
/// Regras de negócio relacionadas à entidade Workflow.
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Cria um novo Workflow.
    /// </summary>
    Task<Workflow> CreateAsync(string name);

    /// <summary>
    /// Obtém um Workflow pelo Id, incluindo seus Nodes.
    /// Lança <see cref="Exceptions.WorkflowNotFoundException"/> caso não exista.
    /// </summary>
    Task<Workflow> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtém todos os Workflows cadastrados.
    /// </summary>
    Task<IEnumerable<Workflow>> GetAllAsync();
}
