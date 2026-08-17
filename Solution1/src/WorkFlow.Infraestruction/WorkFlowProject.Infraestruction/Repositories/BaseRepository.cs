using System.Data;
using Dapper;
using WorkFlowProject.Domain.Interfaces.Repositories;
using WorkFlowProject.Infraestruction.Data;

namespace WorkFlowProject.Infraestruction.Repositories;

/// <summary>
/// Classe base responsável por centralizar o acesso a dados utilizando Dapper.
/// Deve ser herdada pelos repositórios concretos de cada agregado/entidade.
/// </summary>
public abstract class BaseRepository : IBaseRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    protected BaseRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Executa uma consulta e retorna o primeiro registro encontrado, ou o valor padrão caso não haja resultado.
    /// </summary>
    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }

    /// <summary>
    /// Executa uma consulta e retorna todos os registros encontrados.
    /// </summary>
    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<T>(sql, parameters);
    }

    /// <summary>
    /// Executa um comando SQL (INSERT, UPDATE, DELETE) e retorna a quantidade de linhas afetadas.
    /// </summary>
    public async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, parameters);
    }
}
