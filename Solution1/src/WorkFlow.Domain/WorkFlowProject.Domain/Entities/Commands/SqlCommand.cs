namespace WorkFlowProject.Domain.Entities.Commands;

/// <summary>
/// Comando de execução SQL, contendo a tabela e os campos envolvidos na operação.
/// </summary>
public class SqlCommand : NodeCommand
{
    /// <summary>
    /// Nome da tabela envolvida na operação SQL.
    /// </summary>
    public string Table { get; private set; }

    /// <summary>
    /// Campos (colunas) envolvidos na operação SQL.
    /// </summary>
    public List<string> Fields { get; private set; }

    public SqlCommand(string table, List<string> fields)
    {
        Table = table;
        Fields = fields;
    }
}
