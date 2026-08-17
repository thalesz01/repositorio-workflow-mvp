using WorkerFlowProject.Worker;
using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Domain.Interfaces.Services;
using WorkFlowProject.Domain.Services;
using WorkFlowProject.Infraestruction.Data;
using WorkFlowProject.Infraestruction.Executors;
using WorkFlowProject.Infraestruction.Repositories;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<IWorkflowExecutionRepository, WorkflowExecutionRepository>();
builder.Services.AddScoped<INodeExecutionRepository, NodeExecutionRepository>();
builder.Services.AddScoped<IWorkflowExecutionLogRepository, WorkflowExecutionLogRepository>();
builder.Services.AddScoped<IWorkflowConfigurationRepository, WorkflowConfigurationRepository>();
builder.Services.AddScoped<IWorkflowExecutionService, WorkflowExecutionService>();
builder.Services.AddScoped<INodeExecutor, SqlNodeExecutor>();
builder.Services.AddScoped<INodeExecutor, HttpNodeExecutor>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
