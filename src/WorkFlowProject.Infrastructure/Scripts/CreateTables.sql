-- Script de criação das tabelas do MVP de Workflow.
-- Execute este script no banco de dados configurado em "ConnectionStrings:DefaultConnection".

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Workflows')
BEGIN
	CREATE TABLE Workflows
	(
		Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
		Name NVARCHAR(200) NOT NULL
	);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowConfigurations')
BEGIN
	CREATE TABLE WorkflowConfigurations
	(
		WorkflowId UNIQUEIDENTIFIER NOT NULL,
		[Key] NVARCHAR(200) NOT NULL,
		Value NVARCHAR(MAX) NOT NULL,
		CONSTRAINT PK_WorkflowConfigurations PRIMARY KEY (WorkflowId, [Key]),
		CONSTRAINT FK_WorkflowConfigurations_Workflows FOREIGN KEY (WorkflowId) REFERENCES Workflows (Id)
	);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowExecutionLogs')
BEGIN
	CREATE TABLE WorkflowExecutionLogs
	(
		Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
		WorkflowExecutionId UNIQUEIDENTIFIER NOT NULL,
		NodeId UNIQUEIDENTIFIER NULL,
		Status INT NOT NULL, -- 1 = Pending, 2 = Running, 3 = Completed, 4 = Failed
		Error NVARCHAR(MAX) NULL,
		CreatedAt DATETIME2 NOT NULL,
		CONSTRAINT FK_WorkflowExecutionLogs_WorkflowExecutions FOREIGN KEY (WorkflowExecutionId) REFERENCES WorkflowExecutions (Id),
		CONSTRAINT FK_WorkflowExecutionLogs_Nodes FOREIGN KEY (NodeId) REFERENCES Nodes (Id)
	);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Nodes')
BEGIN
	CREATE TABLE Nodes
	(
		Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
		WorkflowId UNIQUEIDENTIFIER NOT NULL,
		Name NVARCHAR(200) NOT NULL,
		[Order] INT NOT NULL,
		Type INT NOT NULL, -- 1 = Sql, 2 = Http
		NextNodeId UNIQUEIDENTIFIER NULL,
		ConnectionStringKey NVARCHAR(200) NULL, -- utilizado apenas por Nodes do tipo Sql
		CommandJson NVARCHAR(MAX) NOT NULL, -- payload serializado do NodeCommand (SqlCommand ou HttpCommand)
		CONSTRAINT FK_Nodes_Workflows FOREIGN KEY (WorkflowId) REFERENCES Workflows (Id)
	);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowExecutions')
BEGIN
	CREATE TABLE WorkflowExecutions
	(
		Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
		WorkflowId UNIQUEIDENTIFIER NOT NULL,
		Status INT NOT NULL, -- 1 = Pending, 2 = Running, 3 = Completed, 4 = Failed
		CurrentNodeId UNIQUEIDENTIFIER NULL,
		StartedAt DATETIME2 NOT NULL,
		FinishedAt DATETIME2 NULL,
		CONSTRAINT FK_WorkflowExecutions_Workflows FOREIGN KEY (WorkflowId) REFERENCES Workflows (Id)
	);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NodeExecutions')
BEGIN
	CREATE TABLE NodeExecutions
	(
		Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
		WorkflowExecutionId UNIQUEIDENTIFIER NOT NULL,
		NodeId UNIQUEIDENTIFIER NOT NULL,
		Status INT NOT NULL, -- 1 = Pending, 2 = Running, 3 = Completed, 4 = Failed
		Input NVARCHAR(MAX) NULL,
		Output NVARCHAR(MAX) NULL,
		Error NVARCHAR(MAX) NULL,
		StartedAt DATETIME2 NOT NULL,
		FinishedAt DATETIME2 NULL,
		CONSTRAINT FK_NodeExecutions_WorkflowExecutions FOREIGN KEY (WorkflowExecutionId) REFERENCES WorkflowExecutions (Id),
		CONSTRAINT FK_NodeExecutions_Nodes FOREIGN KEY (NodeId) REFERENCES Nodes (Id)
	);
END
GO

