using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Enums;
using WorkFlowProject.Domain.Exceptions;
using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Domain.Interfaces.Services;

namespace WorkFlowProject.Domain.Services;

/// <summary>
/// Orquestra a execução de um Workflow, etapa a etapa, delegando o processamento de cada Node
/// ao INodeExecutor correspondente e controlando o avanço da WorkflowExecution na cadeia de Nodes.
/// </summary>
public class WorkflowExecutionService(
    IWorkflowExecutionRepository workflowExecutionRepository,
    INodeExecutionRepository nodeExecutionRepository,
    IWorkflowRepository workflowRepository,
    IWorkflowConfigurationRepository workflowConfigurationRepository,
    IWorkflowExecutionLogRepository workflowExecutionLogRepository,
    IEnumerable<INodeExecutor> nodeExecutors) : IWorkflowExecutionService
{
    private readonly IWorkflowExecutionRepository _workflowExecutionRepository = workflowExecutionRepository;
    private readonly INodeExecutionRepository _nodeExecutionRepository = nodeExecutionRepository;
    private readonly IWorkflowRepository _workflowRepository = workflowRepository;
    private readonly IWorkflowConfigurationRepository _workflowConfigurationRepository = workflowConfigurationRepository;
    private readonly IWorkflowExecutionLogRepository _workflowExecutionLogRepository = workflowExecutionLogRepository;
    private readonly IEnumerable<INodeExecutor> _nodeExecutors = nodeExecutors;

    // Grava a execução do workflow e inicia a execução do primeiro node.
    public async Task<WorkflowExecution> StartExecutionAsync(Guid workflowId)
    {
        var workflow = await _workflowRepository.GetByIdAsync(workflowId)
            ?? throw new WorkflowNotFoundException(workflowId);

        var firstNode = workflow.Nodes.OrderBy(n => n.Order).FirstOrDefault();

        var execution = new WorkflowExecution(workflowId, firstNode?.Id);

        if (firstNode is null)
        {
            execution.MoveToNextNode(null);
        }

        await _workflowExecutionRepository.CreateAsync(execution);
        await _workflowExecutionLogRepository.CreateAsync(
            new WorkflowExecutionLog(execution.Id, execution.CurrentNodeId, execution.Status));

        return execution;
    }

    public async Task<WorkflowExecution> GetExecutionAsync(Guid executionId) =>
        await _workflowExecutionRepository.GetByIdAsync(executionId)
            ?? throw new WorkflowExecutionNotFoundException(executionId);

    public async Task<IEnumerable<WorkflowExecutionLog>> GetExecutionLogsAsync(Guid executionId)
    {
        await GetExecutionAsync(executionId);
        return await _workflowExecutionLogRepository.GetByWorkflowExecutionIdAsync(executionId);
    }

    public Task<IEnumerable<WorkflowExecution>> GetPendingExecutionsAsync() =>
        _workflowExecutionRepository.GetPendingExecutionsAsync();

    public async Task ExecuteNextStepAsync(Guid executionId)
    {
        var execution = await _workflowExecutionRepository.GetByIdAsync(executionId)
            ?? throw new WorkflowExecutionNotFoundException(executionId);

        if (execution.CurrentNodeId is null || execution.Status is ExecutionStatus.Completed or ExecutionStatus.Failed)
        {
            return;
        }

        var workflow = await _workflowRepository.GetByIdAsync(execution.WorkflowId)
            ?? throw new WorkflowNotFoundException(execution.WorkflowId);
        var nodes = workflow.Nodes.ToDictionary(node => node.Id);

        // Vou retirar e colocar para preencher o input do node com o output do último node executado, caso exista.
        var lastNodeExecution = await _nodeExecutionRepository.GetLastByWorkflowExecutionIdAsync(execution.Id);
        var input = lastNodeExecution?.Output;

        while (execution.CurrentNodeId is Guid currentNodeId)
        {
            var node = nodes.GetValueOrDefault(currentNodeId)
                ?? throw new NodeNotFoundException(currentNodeId);

            var executor = _nodeExecutors.FirstOrDefault(executor => executor.CanExecute(node))
                ?? throw new InvalidOperationException($"Nenhum executor encontrado para o Node do tipo '{node.Type}'.");

            execution.MarkAsRunning();
            await _workflowExecutionRepository.UpdateAsync(execution);
            await _workflowExecutionLogRepository.CreateAsync(
                new WorkflowExecutionLog(execution.Id, node.Id, execution.Status));

            var nodeExecution = new NodeExecution(execution.Id, node.Id, input);
            await _nodeExecutionRepository.CreateAsync(nodeExecution);

            try
            {
                input = await executor.ExecuteAsync(node, input);
                nodeExecution.Complete(input);
                await _nodeExecutionRepository.UpdateAsync(nodeExecution);

                execution.MoveToNextNode(node.NextNodeId);
                await _workflowExecutionRepository.UpdateAsync(execution);
                await _workflowExecutionLogRepository.CreateAsync(
                    new WorkflowExecutionLog(execution.Id, node.Id, execution.Status));
            }
            catch (Exception ex)
            {
                nodeExecution.Fail(ex.Message);
                await _nodeExecutionRepository.UpdateAsync(nodeExecution);

                execution.MarkAsFailed();
                await _workflowExecutionRepository.UpdateAsync(execution);
                await _workflowExecutionLogRepository.CreateAsync(
                    new WorkflowExecutionLog(execution.Id, node.Id, execution.Status, ex.Message));
                return;
            }
        }
    }
}
