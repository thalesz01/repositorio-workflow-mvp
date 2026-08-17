using System.Data;

namespace WorkFlowProject.Infraestruction.Data;

/// <summary>
/// Responsável por criar conexões com o banco de dados.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Cria e retorna uma nova conexão com o banco de dados.
    /// A conexão retornada não está aberta; o chamador é responsável por abri-la.
    /// </summary>
    IDbConnection CreateConnection();
}
