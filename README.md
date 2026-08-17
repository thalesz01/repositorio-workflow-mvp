# repositorio-workflow-mvp

Repositório para o projeto MVP do workflow de projetos.

## Estrutura

```
WorkFlow-Project.slnx
src/
  WorkFlowProject.API/             API REST (controllers, DTOs)
  WorkFlowProject.Domain/          Entidades, enums, interfaces, serviços de domínio
  WorkFlowProject.Infrastructure/  Dapper, repositórios, executores de nó, scripts SQL
  WorkFlowProject.Worker/          Worker de execução em background
```

Visão detalhada da arquitetura e do modelo de dados: [OVERVIEW.md](OVERVIEW.md).

## Configuração local

A connection string **não é versionada**. Configure-a localmente com User Secrets
(um comando por projeto, na pasta do projeto):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=localhost;Initial Catalog=WORKFLOW_PROJECT;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
```

Alternativa por variável de ambiente:

```bash
setx ConnectionStrings__DefaultConnection "Data Source=localhost;Initial Catalog=WORKFLOW_PROJECT;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
```

Se preferir autenticação SQL em vez de `Integrated Security`, troque por
`User ID=...;Password=...` — mas **nunca** commite essa string.

O schema do banco está em
`src/WorkFlowProject.Infrastructure/Scripts/CreateTables.sql`.

## Build e execução

```bash
dotnet build WorkFlow-Project.slnx
```

```bash
dotnet run --project src/WorkFlowProject.API
```

```bash
dotnet run --project src/WorkFlowProject.Worker
```

Em Development, a documentação da API fica disponível em `/swagger` (Scalar).
