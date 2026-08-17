# Overview — WorkFlow Project MVP

## Objetivo

Este projeto implementa um **motor de workflows sequenciais**. Um workflow é composto por uma cadeia ordenada de nodes, em que cada node executa uma ação e pode fornecer sua saída como entrada para a próxima etapa.

O MVP suporta dois tipos de node:

- **SQL (`SqlNode`)**: consulta uma tabela do SQL Server e retorna um registro em JSON.
- **HTTP (`HttpNode`)**: realiza uma requisição HTTP e pode montar o corpo da requisição usando valores retornados pela etapa anterior.

A API cria e consulta a definição dos workflows e solicita sua execução. Um Worker separado processa as execuções de forma assíncrona, node a node.

---

## Arquitetura

A solução está organizada seguindo uma separação inspirada em DDD e Repository Pattern.

| Projeto | Responsabilidade |
|---|---|
| `WorkFlowProject.Domain` | Regras de negócio, entidades, enums, contratos de repositórios e serviços. Não depende das outras camadas. |
| `WorkFlowProject.Infraestruction` | Persistência com Dapper/SQL Server, fábrica de conexões e implementações dos executores de nodes. |
| `WorkFlowProject.API` | API ASP.NET Core que expõe operações para criar, consultar e iniciar workflows. |
| `WorkerFlowProject.Worker` | Serviço em segundo plano que localiza execuções pendentes e processa uma etapa por vez. |

### Direção das dependências

```text
API ───────────────┐
Worker ────────────┼──> Infrastructure ───> Domain
				   │
				   └──────────────────────> Domain
```

O `Domain` contém as abstrações. A `Infrastructure` implementa essas abstrações. API e Worker registram as implementações no container de injeção de dependência.

---

## Modelo de domínio

### Workflow

Representa a definição de um processo.

Principais campos:

- `Id`: identificador único (`Guid`).
- `Name`: nome do workflow.
- `Nodes`: coleção de etapas do processo.

Um workflow é criado sem nodes. Os nodes são incluídos posteriormente e passam a formar uma cadeia de execução.

### Node

Representa uma etapa executável de um workflow.

Principais campos:

- `Id`: identificador único (`Guid`).
- `WorkflowId`: workflow ao qual pertence.
- `Name`: nome da etapa.
- `Order`: posição sequencial do node.
- `NextNodeId`: identificador da próxima etapa. É `null` no último node.
- `Type`: `Sql` ou `Http`.
- `Command`: configuração específica da ação a executar.

Ao adicionar um node, o `NodeService`:

1. Confirma que o workflow existe.
2. Localiza o último node existente.
3. Define a próxima ordem.
4. Persiste o novo node.
5. Atualiza o `NextNodeId` do node anterior para apontar para o novo node.

Assim, a execução é orientada pelo encadeamento `NextNodeId`, mantendo também a ordenação para consultas e para descobrir a primeira etapa.

### SqlNode e SqlCommand

Um `SqlNode` possui:

- `ConnectionStringKey`: chave da connection string configurada, por exemplo `DefaultConnection`.
- `SqlCommand.Table`: tabela consultada.
- `SqlCommand.Fields`: campos retornados pela consulta.

No estado atual do MVP, a consulta montada é conceitualmente:

```sql
SELECT TOP 1 [Campo1], [Campo2]
FROM [Tabela]
```

O resultado é serializado como JSON e registrado como saída da execução do node.

### HttpNode e HttpCommand

Um `HttpNode` possui:

- `Url`: endereço da requisição.
- `Method`: `Get`, `Post`, `Put`, `Delete` ou `Patch`.
- `Headers`: cabeçalhos opcionais.
- `Body`: corpo opcional da requisição.

O corpo suporta placeholders no formato `{{campo}}`. Durante a execução, os valores são obtidos do JSON produzido pelo node anterior.

Exemplo:

```json
{
  "Body": "{\"customerId\": \"{{Id}}\", \"name\": \"{{Name}}\"}"
}
```

Se a saída da etapa anterior for:

```json
{
  "Id": 10,
  "Name": "Cliente A"
}
```

O corpo enviado pelo node HTTP será:

```json
{
  "customerId": "10",
  "name": "Cliente A"
}
```

### WorkflowExecution

Representa uma instância de execução de um workflow, separada da sua definição.

Principais campos:

- `Id`: identificador da execução.
- `WorkflowId`: workflow que está sendo executado.
- `Status`: `Pending`, `Running`, `Completed` ou `Failed`.
- `CurrentNodeId`: node que deverá ser processado no próximo ciclo.
- `StartedAt` e `FinishedAt`: timestamps do ciclo de vida.

### NodeExecution

Representa o histórico de uma etapa em uma execução específica.

Principais campos:

- `WorkflowExecutionId`: execução principal à qual pertence.
- `NodeId`: node executado.
- `Input`: saída JSON recebida do node anterior.
- `Output`: resultado produzido pelo node.
- `Error`: mensagem de erro, quando houver.
- `Status`, `StartedAt` e `FinishedAt`.

---

## Persistência

A persistência usa **Dapper** e SQL Server. A infraestrutura possui uma `IDbConnectionFactory`, implementada por `SqlConnectionFactory`, que usa a connection string `ConnectionStrings:DefaultConnection`.

A classe `BaseRepository` concentra as operações assíncronas reutilizáveis:

- `QueryFirstOrDefaultAsync<T>`
- `QueryAsync<T>`
- `ExecuteAsync`

Os comandos específicos de node são persistidos na tabela `Nodes` através da coluna `CommandJson`.

### Tabelas

O script `src/WorkFlow.Infraestruction/WorkFlowProject.Infraestruction/Scripts/CreateTables.sql` cria as tabelas abaixo.

| Tabela | Finalidade |
|---|---|
| `Workflows` | Definição básica de cada workflow. |
| `Nodes` | Nodes do workflow, sua ordem, encadeamento e comando serializado. |
| `WorkflowExecutions` | Estado de cada solicitação de execução. |
| `NodeExecutions` | Histórico, entrada, saída e erro de cada node executado. |

Relações principais:

```text
Workflows 1 ─── N Nodes
Workflows 1 ─── N WorkflowExecutions
WorkflowExecutions 1 ─── N NodeExecutions
Nodes 1 ─── N NodeExecutions
```

---

## Serviços de domínio

### WorkflowService

Centraliza regras de workflows:

- Criar workflow.
- Buscar workflow por identificador.
- Listar workflows.
- Lançar `WorkflowNotFoundException` quando necessário.

### NodeService

Centraliza regras de nodes:

- Criar node SQL.
- Criar node HTTP.
- Buscar node por identificador.
- Listar nodes de um workflow.
- Calcular ordem e encadear automaticamente nodes por `NextNodeId`.

### WorkflowExecutionService

Orquestra o ciclo de vida das execuções:

- Cria uma `WorkflowExecution` com status `Pending`.
- Define o primeiro node como etapa atual.
- Finaliza imediatamente uma execução cujo workflow não possui nodes.
- Carrega execuções pendentes ou em andamento.
- Executa o node atual por meio de um `INodeExecutor` compatível.
- Salva `NodeExecution` com input, output ou erro.
- Avança para o próximo node ou finaliza a execução.
- Em caso de erro, marca o node e o workflow como `Failed`.

### INodeExecutor

É a abstração para executar nodes. Cada implementação declara se suporta um node por `CanExecute(Node)` e executa a etapa por `ExecuteAsync(Node, input)`.

Implementações atuais:

| Executor | Node suportado | Comportamento |
|---|---|---|
| `SqlNodeExecutor` | `SqlNode` | Executa um `SELECT TOP 1` no SQL Server e retorna JSON. |
| `HttpNodeExecutor` | `HttpNode` | Envia a requisição HTTP e retorna o conteúdo da resposta. |

---

## API REST

Base: `/api/workflows`

| Método | Rota | Finalidade |
|---|---|---|
| `GET` | `/api/workflows` | Lista todos os workflows. |
| `POST` | `/api/workflows` | Cria um workflow. |
| `GET` | `/api/workflows/{id}` | Obtém um workflow com seus nodes. |
| `GET` | `/api/workflows/{workflowId}/nodes` | Lista todos os nodes de um workflow. |
| `POST` | `/api/workflows/{workflowId}/nodes` | Inclui um node SQL ou HTTP. |
| `GET` | `/api/workflows/{workflowId}/nodes/{nodeId}` | Obtém um node específico. |
| `POST` | `/api/workflows/{workflowId}/execute` | Cria uma execução pendente para o Worker processar. |
| `GET` | `/api/workflows/{workflowId}/executions/{executionId}` | Consulta o estado de uma execução. |

### Exemplo: criar workflow

```http
POST /api/workflows
Content-Type: application/json

{
  "name": "Consultar cliente e notificar sistema externo"
}
```

### Exemplo: adicionar node SQL

```http
POST /api/workflows/{workflowId}/nodes
Content-Type: application/json

{
  "name": "Buscar cliente",
  "type": "Sql",
  "connectionStringKey": "DefaultConnection",
  "table": "Customers",
  "fields": ["Id", "Name", "Email"]
}
```

### Exemplo: adicionar node HTTP

```http
POST /api/workflows/{workflowId}/nodes
Content-Type: application/json

{
  "name": "Notificar sistema externo",
  "type": "Http",
  "url": "https://example.com/api/notifications",
  "method": "Post",
  "headers": {
	"X-Source": "workflow-mvp"
  },
  "body": "{\"customerId\": \"{{Id}}\", \"email\": \"{{Email}}\"}"
}
```

### Exemplo: iniciar execução

```http
POST /api/workflows/{workflowId}/execute
```

A resposta contém uma execução inicialmente `Pending`. A API não executa os nodes de forma síncrona.

---

## Fluxo esperado de execução

### Fluxo principal

```text
Cliente
  │
  ├── POST /api/workflows/{workflowId}/execute
  │
API
  │
  ├── WorkflowExecutionService.StartExecutionAsync
  │     ├── valida o workflow
  │     ├── identifica o primeiro node
  │     └── grava WorkflowExecution com status Pending
  │
SQL Server
  │
  └── WorkflowExecutions
		  │
		  ▼
Worker (a cada 2 segundos)
  │
  ├── busca execuções Pending ou Running
  ├── para cada execução, processa apenas o CurrentNodeId
  │     │
  │     ├── cria NodeExecution com status Running
  │     ├── seleciona SqlNodeExecutor ou HttpNodeExecutor
  │     ├── executa o node
  │     ├── grava Output ou Error no NodeExecution
  │     └── atualiza CurrentNodeId e Status do WorkflowExecution
  │
  └── próximo ciclo do Worker processa a próxima etapa
```

### Passo a passo detalhado

1. Um consumidor cria um workflow pela API.
2. O consumidor adiciona nodes. Cada inclusão é conectada automaticamente ao node anterior.
3. O consumidor chama `POST /api/workflows/{workflowId}/execute`.
4. A API cria uma `WorkflowExecution`:
   - `Pending` quando existir node inicial;
   - `Completed` imediatamente se não houver nodes.
5. O Worker faz polling no banco a cada dois segundos.
6. Para cada execução `Pending` ou `Running`, o Worker chama `ExecuteNextStepAsync`.
7. O serviço muda a execução para `Running`, cria um registro `NodeExecution` e carrega a saída da última etapa como `Input`.
8. O executor compatível processa o node atual:
   - SQL: retorna o primeiro registro como JSON.
   - HTTP: substitui placeholders no body usando o JSON de entrada e envia a requisição.
9. Em caso de sucesso:
   - `NodeExecution` fica `Completed` e recebe o `Output`.
   - `WorkflowExecution.CurrentNodeId` recebe o `NextNodeId`.
   - Caso não haja próximo node, a execução passa para `Completed`.
10. Em caso de falha:
	- `NodeExecution` fica `Failed` e registra a mensagem em `Error`.
	- `WorkflowExecution` fica `Failed` e recebe `FinishedAt`.
11. O status pode ser consultado pela rota de execução.

---

## Estados possíveis

| Status | Significado |
|---|---|
| `Pending` | Execução criada e aguardando processamento pelo Worker. |
| `Running` | Pelo menos uma etapa já começou; a próxima etapa será processada em um ciclo posterior. |
| `Completed` | Não há próximo node e todas as etapas concluíram com sucesso. |
| `Failed` | Um node lançou erro ou retornou resposta HTTP não bem-sucedida. |

---

## Configuração e execução local

1. Configure `ConnectionStrings:DefaultConnection` nos arquivos `appsettings.json` da API e do Worker.
2. Execute o script `CreateTables.sql` no SQL Server apontado pela connection string.
3. Inicie a API `WorkFlowProject.API`.
4. Inicie o Worker `WorkerFlowProject.Worker`.
5. Crie o workflow e seus nodes pela API.
6. Solicite a execução pela rota `POST /execute`.
7. Consulte a execução pela rota `GET /executions/{executionId}`.

A API e o Worker precisam apontar para o mesmo banco de dados, pois a tabela `WorkflowExecutions` é o mecanismo atual de comunicação assíncrona entre eles.

---

## Limitações atuais do MVP

- O Worker utiliza polling simples a cada dois segundos; não há fila ou mensageria.
- Não há mecanismo de lock/claim de execução. Mais de uma instância de Worker pode processar a mesma execução simultaneamente.
- Não há retentativa automática, timeout configurável, cancelamento, agendamento ou backoff.
- O node SQL aceita nomes de tabela e campos configurados pelo consumidor. Em uma evolução de produção, esses identificadores devem ser validados por allowlist/metadados para reduzir riscos de consultas dinâmicas indevidas.
- O SQL executa apenas `SELECT TOP 1`, sem filtros, parâmetros ou paginação.
- Placeholders são substituídos apenas no corpo HTTP, a partir de propriedades no nível raiz de um JSON válido.
- Não há autenticação, autorização, versionamento de workflow ou auditoria de usuários.
- A saída de uma resposta HTTP é armazenada como texto; se não for JSON, os placeholders do node seguinte não encontrarão propriedades para substituir.
- Não existem testes automatizados no escopo atual.

---

## Próximas evoluções sugeridas

1. Adicionar testes unitários para serviços de domínio e testes de integração para repositórios/executores.
2. Adicionar lock transacional ou fila para evitar execução concorrente do mesmo workflow.
3. Implementar retentativas, timeout, política de erro e cancelamento.
4. Parametrizar nodes SQL e validar tabelas/campos permitidos.
5. Permitir placeholders também em URL, headers e parâmetros SQL.
6. Registrar logs estruturados por `WorkflowExecutionId` e `NodeExecutionId`.
7. Expor endpoint para listar histórico de `NodeExecutions`.
8. Adicionar autenticação, autorização, versionamento e governança dos workflows.
