using WorkFlowProject.Domain.Interfaces.Services;

namespace WorkerFlowProject.Worker;

public class Worker(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<Worker> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker de execução de Workflows iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecutePendingWorkflowsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar execuções pendentes de Workflow.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ExecutePendingWorkflowsAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var executionService = scope.ServiceProvider.GetRequiredService<IWorkflowExecutionService>();
        var executions = await executionService.GetPendingExecutionsAsync();

        foreach (var execution in executions)
        {
            stoppingToken.ThrowIfCancellationRequested();

            logger.LogInformation(
                "Executando WorkflowExecution {ExecutionId} a partir do Node atual {NodeId}.",
                execution.Id,
                execution.CurrentNodeId);

            await executionService.ExecuteNextStepAsync(execution.Id);
        }
    }
}
