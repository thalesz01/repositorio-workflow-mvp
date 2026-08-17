using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace WorkFlowProject.Infraestruction.Data;

/// <summary>
/// Implementação de IDbConnectionFactory para SQL Server, utilizando a connection string
/// configurada em "ConnectionStrings:DefaultConnection".
/// </summary>
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não foi configurada.");
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
