using System.Text.Json;
using System.Text.RegularExpressions;

namespace WorkFlowProject.Domain.Services;

/// <summary>
/// Utilitário responsável por substituir placeholders no formato "{{chave}}" em um texto,
/// utilizando os valores de um JSON plano (Output do Node anterior) como fonte de dados.
/// </summary>
public static partial class PlaceholderResolver
{
    private static readonly Regex PlaceholderPattern = new(@"\{\{\s*(\w+)\s*\}\}", RegexOptions.Compiled);

    /// <summary>
    /// Substitui os placeholders "{{chave}}" encontrados no texto pelos valores correspondentes
    /// presentes no JSON informado. Placeholders sem correspondência são mantidos como estão.
    /// </summary>
    public static string Resolve(string text, string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return text;
        }

        Dictionary<string, string>? values;
        try
        {
            var document = JsonDocument.Parse(json);
            values = document.RootElement.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.ToString());
        }
        catch (JsonException)
        {
            return text;
        }

        return PlaceholderPattern.Replace(text, match =>
        {
            var key = match.Groups[1].Value;
            return values.TryGetValue(key, out var value) ? value : match.Value;
        });
    }
}
