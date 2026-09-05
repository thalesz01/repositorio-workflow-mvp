namespace WorkFlowProject.Domain.Enums;

/// <summary>
/// Define o tipo de execução que um Node do Workflow realiza.
/// </summary>
public enum NodeType
{
    /// <summary>
    /// Execução de um comando/consulta SQL.
    /// </summary>
    Sql = 1,

    /// <summary>
    /// Execução de uma requisição HTTP.
    /// </summary>
    Http = 2
}
