using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Domain.Interfaces.Services;
using WorkFlowProject.Domain.Services;
using WorkFlowProject.Infraestruction.Data;
using WorkFlowProject.Infraestruction.Executors;
using WorkFlowProject.Infraestruction.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<IWorkflowExecutionRepository, WorkflowExecutionRepository>();
builder.Services.AddScoped<INodeExecutionRepository, NodeExecutionRepository>();
builder.Services.AddScoped<IWorkflowExecutionLogRepository, WorkflowExecutionLogRepository>();
builder.Services.AddScoped<IWorkflowConfigurationRepository, WorkflowConfigurationRepository>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<INodeService, NodeService>();
builder.Services.AddScoped<IWorkflowExecutionService, WorkflowExecutionService>();
builder.Services.AddScoped<INodeExecutor, SqlNodeExecutor>();
builder.Services.AddScoped<INodeExecutor, HttpNodeExecutor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/swagger");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
