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

### 1. Criar o seu appsettings.json

Os `appsettings.json` **não são versionados** — eles carregam connection strings com
credenciais. O repositório traz apenas o modelo. Copie-o em cada projeto e preencha:

```bash
cp src/WorkFlowProject.API/appsettings.Example.json src/WorkFlowProject.API/appsettings.json
```

```bash
cp src/WorkFlowProject.Worker/appsettings.Example.json src/WorkFlowProject.Worker/appsettings.json
```

Alternativa sem tocar no arquivo: mantenha o `Password=` vazio e informe a senha por
`dotnet user-secrets` (ver passo 3).

### 2. Criar o login da aplicação

A aplicação **não usa o `sa`**. Ela se conecta com um login dedicado que tem apenas
leitura e escrita no banco `WorkFlow_Project` — se a credencial vazar, o estrago
fica contido a este banco em vez da instância inteira.

Rode uma vez, a partir de `src/WorkFlowProject.Infrastructure/Scripts`:

```bash
sqlcmd -S localhost,1433 -U sa -i CreateAppLogin.sql -v AppLogin="workflow_app" -v AppPassword="SuaSenhaForte#2026"
```

O script é idempotente. A senha não fica no arquivo: entra por variável do `sqlcmd`.

### 3. Informar a senha à aplicação

O `appsettings.json` já traz a connection string com `User ID=workflow_app`, mas com
`Password=` **vazio** de propósito, para que nenhuma credencial seja versionada.

Passe a senha por User Secrets. Atenção: o User Secrets substitui a chave
`ConnectionStrings:DefaultConnection` **inteira**, não apenas a senha — então repita
a string completa. Rode na pasta de cada projeto (`src/WorkFlowProject.API` e
`src/WorkFlowProject.Worker`):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=localhost;Initial Catalog=WorkFlow_Project;User ID=workflow_app;Password=SuaSenhaForte#2026;Pooling=False;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Authentication=SqlPassword;Application Name=vscode-mssql;Application Intent=ReadWrite;Command Timeout=30"
```

Alternativa por variável de ambiente (o `__` equivale ao `:` da chave):

```bash
setx ConnectionStrings__DefaultConnection "Data Source=localhost;Initial Catalog=WorkFlow_Project;User ID=workflow_app;Password=SuaSenhaForte#2026;..."
```

**Nunca** preencha a senha direto no `appsettings.json` — esse arquivo é versionado.

### Schema

O schema do banco está em
`src/WorkFlowProject.Infrastructure/Scripts/CreateTables.sql`. Rode-o **antes** do
`CreateAppLogin.sql`, já que o login precisa que o banco `WorkFlow_Project` exista.

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
