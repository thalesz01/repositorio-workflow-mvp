using System.Net.Http.Headers;
using System.Text;
using WorkFlowProject.Domain.Entities;
using WorkFlowProject.Domain.Entities.Commands;
using WorkFlowProject.Domain.Enums;
using WorkFlowProject.Domain.Interfaces.Services;
using WorkFlowProject.Domain.Services;

namespace WorkFlowProject.Infrastructure.Executors;

/// <summary>
/// Executor responsável por processar Nodes do tipo HTTP, realizando a requisição configurada
/// no HttpCommand, substituindo placeholders "{{campo}}" do body com base no Input recebido.
/// </summary>
public class HttpNodeExecutor : INodeExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpNodeExecutor(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public bool CanExecute(Node node) => node.Type == NodeType.Http;

    public async Task<string?> ExecuteAsync(Node node, string? input)
    {
        HttpNode httpNode = (HttpNode)node;
        HttpCommand command = httpNode.HttpCommand;

        HttpClient client = _httpClientFactory.CreateClient(nameof(HttpNodeExecutor));

        string? body = command.Body is null ? null : PlaceholderResolver.Resolve(command.Body, input);

        using HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(command.Method.ToString()), command.Url);

        foreach (KeyValuePair<string, string> header in command.Headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (body is not null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        using HttpResponseMessage response = await client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return content;
    }
}
