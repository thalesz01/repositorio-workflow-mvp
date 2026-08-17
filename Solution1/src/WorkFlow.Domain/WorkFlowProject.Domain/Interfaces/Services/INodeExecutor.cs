using WorkFlowProject.Domain.Entities;

namespace WorkFlowProject.Domain.Interfaces.Services;

/// <summary>
/// Responsável por executar um Node específico, produzindo um resultado (Output) em formato JSON,
/// a partir dos dados de entrada (Input) recebidos do Node anterior na cadeia de execução.
/// </summary>
public interface INodeExecutor
{
    /// <summary>
    /// Indica se este executor é capaz de processar o Node informado.
    /// </summary>
    bool CanExecute(Node node);

    /// <summary>
    /// Executa o Node e retorna o resultado produzido, serializado em JSON.
    /// </summary>
    /// <param name="node">Node a ser executado.</param>
    /// <param name="input">Dados de entrada (Output do Node anterior), em formato JSON. Pode ser nulo para o primeiro Node.</param>
    Task<string?> ExecuteAsync(Node node, string? input);
}
