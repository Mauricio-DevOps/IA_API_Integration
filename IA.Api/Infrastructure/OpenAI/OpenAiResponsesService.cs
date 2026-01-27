using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IA.Api.Application.Contracts;
using IA.Api.Domain.Models;
using IA.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace IA.Api.Infrastructure.OpenAI;

public sealed class OpenAiResponsesService : IOpenAiResponsesService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiResponsesService(HttpClient httpClient, IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }
    }

    public async Task<AssistantResponse> CreateResponseAsync(
        ResponsesCommand command,
        CancellationToken cancellationToken = default)
    {
        EnsureConfiguration();

        var payload = SerializePayload(command);

        using var request = new HttpRequestMessage(HttpMethod.Post, "responses")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"OpenAI responses API returned {(int)response.StatusCode} {response.StatusCode}: {errorContent}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var responseId = document.RootElement.GetProperty("id").GetString() ?? string.Empty;
        var outputText = ExtractOutputText(document.RootElement);

        return new AssistantResponse(responseId, outputText)
        {
            Raw = document.RootElement.Clone()
        };
    }

    private string SerializePayload(ResponsesCommand command)
    {
        var body = new Dictionary<string, object?>
        {
            ["model"] = _options.ResponsesModel,
            ["instructions"] = _options.SystemPrompt,
            ["input"] = command.Prompt
        };

        if (command.UseFileSearch && _options.VectorStoreIds.Count > 0)
        {
            body["tools"] = new[]
            {
                new
                {
                    type = "file_search",
                    vector_store_ids = _options.VectorStoreIds
                }
            };

            body["include"] = new[]
            {
                "file_search_call.results"
            };
        }

        if (!string.IsNullOrWhiteSpace(command.PreviousResponseId))
        {
            body["previous_response_id"] = command.PreviousResponseId;
        }

        return JsonSerializer.Serialize(body, SerializerOptions);
    }

    private static string ExtractOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output", out var outputElements))
        {
            foreach (var item in outputElements.EnumerateArray())
            {
                if (item.TryGetProperty("content", out var contents))
                {
                    foreach (var content in contents.EnumerateArray())
                    {
                        if (content.TryGetProperty("type", out var typeProperty)
                            && typeProperty.GetString() == "output_text"
                            && content.TryGetProperty("text", out var textProperty))
                        {
                            return textProperty.GetString() ?? string.Empty;
                        }
                    }
                }
            }
        }

        return string.Empty;
    }

    private void EnsureConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.ResponsesModel))
        {
            throw new InvalidOperationException("OpenAI responses model is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.SystemPrompt))
        {
            throw new InvalidOperationException("OpenAI system prompt is not configured.");
        }
    }
}
