using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Exceptions;
using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Domain.Interfaces.Services;

namespace WorkFlowProject.Domain.Services;

/// <summary>
/// Implementa as regras de negócio relacionadas à entidade Workflow.
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly IWorkflowRepository _workflowRepository;

    public WorkflowService(IWorkflowRepository workflowRepository)
    {
        _workflowRepository = workflowRepository;
    }

    public async Task<Workflow> CreateAsync(string name)
    {
        var workflow = new Workflow(name);
        await _workflowRepository.CreateAsync(workflow);
        return workflow;
    }

    public async Task<Workflow> GetByIdAsync(Guid id)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id);
        return workflow ?? throw new WorkflowNotFoundException(id);
    }

    public Task<IEnumerable<Workflow>> GetAllAsync() => _workflowRepository.GetAllAsync();
}
