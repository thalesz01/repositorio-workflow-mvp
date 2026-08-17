namespace WorkFlowProject.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato base para repositórios que realizam acesso a dados.
/// Define as operações genéricas de consulta e execução de comandos.
/// </summary>
public interface IBaseRepository
{
    /// <summary>
    /// Executa uma consulta e retorna o primeiro registro encontrado, ou o valor padrão caso não haja resultado.
    /// </summary>
    /// <typeparam name="T">Tipo do objeto retornado pela consulta.</typeparam>
    /// <param name="sql">Comando SQL a ser executado.</param>
    /// <param name="parameters">Parâmetros do comando SQL.</param>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null);

    /// <summary>
    /// Executa uma consulta e retorna todos os registros encontrados.
    /// </summary>
    /// <typeparam name="T">Tipo do objeto retornado pela consulta.</typeparam>
    /// <param name="sql">Comando SQL a ser executado.</param>
    /// <param name="parameters">Parâmetros do comando SQL.</param>
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);

    /// <summary>
    /// Executa um comando SQL (INSERT, UPDATE, DELETE) e retorna a quantidade de linhas afetadas.
    /// </summary>
    /// <param name="sql">Comando SQL a ser executado.</param>
    /// <param name="parameters">Parâmetros do comando SQL.</param>
    Task<int> ExecuteAsync(string sql, object? parameters = null);
}
