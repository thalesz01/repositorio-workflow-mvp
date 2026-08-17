using Microsoft.AspNetCore.Mvc;
using WorkFlowProject.API.Dtos;
using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Enums;
using WorkFlowProject.Domain.Exceptions;
using WorkFlowProject.Domain.Interfaces.Services;

namespace WorkFlowProject.API.Controllers;

[ApiController]
[Route("api/workflows")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly INodeService _nodeService;
    private readonly IWorkflowExecutionService _workflowExecutionService;

    public WorkflowsController(IWorkflowService workflowService, INodeService nodeService, IWorkflowExecutionService workflowExecutionService)
    {
        _workflowService = workflowService;
        _nodeService = nodeService;
        _workflowExecutionService = workflowExecutionService;
    }

    /// <summary>
    /// Lista todos os Workflows cadastrados.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowResponse>>> GetAll()
    {
        var workflows = await _workflowService.GetAllAsync();
        return Ok(workflows.Select(ToResponse));
    }

    /// <summary>
    /// Consulta um Workflow pelo Id, incluindo seus Nodes.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkflowResponse>> GetById(Guid id)
    {
        try
        {
            var workflow = await _workflowService.GetByIdAsync(id);
            return Ok(ToResponse(workflow));
        }
        catch (WorkflowNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Cria um novo Workflow (sem Nodes).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkflowResponse>> Create([FromBody] CreateWorkflowRequest request)
    {
        var workflow = await _workflowService.CreateAsync(request.Name);
        return CreatedAtAction(nameof(GetById), new { id = workflow.Id }, ToResponse(workflow));
    }

    /// <summary>
    /// Lista os Nodes de um Workflow.
    /// </summary>
    [HttpGet("{workflowId:guid}/nodes")]
    public async Task<ActionResult<IEnumerable<NodeResponse>>> GetNodes(Guid workflowId)
    {
        try
        {
            await _workflowService.GetByIdAsync(workflowId);
            var nodes = await _nodeService.GetByWorkflowIdAsync(workflowId);
            return Ok(nodes.Select(ToResponse));
        }
        catch (WorkflowNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Consulta um Node específico pelo Id.
    /// </summary>
    [HttpGet("{workflowId:guid}/nodes/{nodeId:guid}")]
    public async Task<ActionResult<NodeResponse>> GetNodeById(Guid workflowId, Guid nodeId)
    {
        try
        {
            var node = await _nodeService.GetByIdAsync(nodeId);

            if (node.WorkflowId != workflowId)
            {
                return NotFound();
            }

            return Ok(ToResponse(node));
        }
        catch (NodeNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Adiciona um novo Node (Sql ou Http) ao final da cadeia de execução de um Workflow existente.
    /// </summary>
    [HttpPost("{workflowId:guid}/nodes")]
    public async Task<ActionResult<NodeResponse>> CreateNode(Guid workflowId, [FromBody] CreateNodeRequest request)
    {
        try
        {
            Node node = request.Type switch
            {
                NodeRequestType.Sql => await _nodeService.CreateSqlNodeAsync(
                    workflowId,
                    request.Name,
                    request.ConnectionStringKey ?? throw new ArgumentException("ConnectionStringKey é obrigatório para Nodes do tipo Sql."),
                    request.Table ?? throw new ArgumentException("Table é obrigatório para Nodes do tipo Sql."),
                    request.Fields ?? throw new ArgumentException("Fields é obrigatório para Nodes do tipo Sql.")),

                NodeRequestType.Http => await _nodeService.CreateHttpNodeAsync(
                    workflowId,
                    request.Name,
                    request.Url ?? throw new ArgumentException("Url é obrigatório para Nodes do tipo Http."),
                    (HttpMethodType)(request.Method ?? throw new ArgumentException("Method é obrigatório para Nodes do tipo Http.")),
                    request.Body,
                    request.Headers),

                _ => throw new ArgumentException($"Tipo de Node inválido: {request.Type}")
            };

            return CreatedAtAction(nameof(GetNodeById), new { workflowId, nodeId = node.Id }, ToResponse(node));
        }
        catch (WorkflowNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Inicia uma nova execução (Pending) para o Workflow informado. O Worker processará as etapas de forma assíncrona.
    /// </summary>
    [HttpPost("{workflowId:guid}/execute")]
    public async Task<ActionResult<WorkflowExecutionResponse>> Execute(Guid workflowId)
    {
        try
        {
            var execution = await _workflowExecutionService.StartExecutionAsync(workflowId);
            return CreatedAtAction(nameof(GetExecution), new { workflowId, executionId = execution.Id }, ToResponse(execution));
        }
        catch (WorkflowNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Consulta o status atual de uma execução de Workflow.
    /// </summary>
    [HttpGet("{workflowId:guid}/executions/{executionId:guid}")]
    public async Task<ActionResult<WorkflowExecutionResponse>> GetExecution(Guid workflowId, Guid executionId)
    {
        try
        {
            var execution = await _workflowExecutionService.GetExecutionAsync(executionId);

            if (execution.WorkflowId != workflowId)
            {
                return NotFound();
            }

            var logs = await _workflowExecutionService.GetExecutionLogsAsync(executionId);
            return Ok(ToResponse(execution, logs));
        }
        catch (WorkflowExecutionNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    private static WorkflowExecutionResponse ToResponse(WorkflowExecution execution) => new()
    {
        Id = execution.Id,
        WorkflowId = execution.WorkflowId,
        Status = execution.Status.ToString(),
        CurrentNodeId = execution.CurrentNodeId,
        StartedAt = execution.StartedAt,
        FinishedAt = execution.FinishedAt
    };

    private static WorkflowExecutionResponse ToResponse(WorkflowExecution execution, IEnumerable<WorkflowExecutionLog> logs)
    {
        var response = ToResponse(execution);
        response.Logs = logs.Select(log => new WorkflowExecutionLogResponse
        {
            Id = log.Id,
            NodeId = log.NodeId,
            Status = log.Status.ToString(),
            Error = log.Error,
            CreatedAt = log.CreatedAt
        });

        return response;
    }

    private static WorkflowResponse ToResponse(Workflow workflow) => new()
    {
        Id = workflow.Id,
        Name = workflow.Name,
        Nodes = workflow.Nodes.Select(ToResponse).ToList()
    };

    private static NodeResponse ToResponse(Node node)
    {
        var response = new NodeResponse
        {
            Id = node.Id,
            WorkflowId = node.WorkflowId,
            Name = node.Name,
            Order = node.Order,
            Type = node.Type.ToString(),
            NextNodeId = node.NextNodeId
        };

        switch (node)
        {
            case SqlNode sqlNode:
                response.ConnectionStringKey = sqlNode.ConnectionStringKey;
                response.Table = sqlNode.SqlCommand.Table;
                response.Fields = sqlNode.SqlCommand.Fields;
                break;

            case HttpNode httpNode:
                response.Url = httpNode.HttpCommand.Url;
                response.Method = httpNode.HttpCommand.Method.ToString();
                response.Headers = httpNode.HttpCommand.Headers;
                response.Body = httpNode.HttpCommand.Body;
                break;
        }

        return response;
    }
}
