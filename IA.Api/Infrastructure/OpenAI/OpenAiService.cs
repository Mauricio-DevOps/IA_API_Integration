using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IA.Api.Application.Contracts;
using IA.Api.Domain.Models;
using IA.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace IA.Api.Infrastructure.OpenAI;

public sealed class OpenAiService : IOpenAiService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiService(HttpClient httpClient, IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }
    }

    public async Task<OpenAiChatCompletionResponse> CreateChatCompletionAsync(
        OpenAiChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureApiKey();

        var payload = JsonSerializer.Serialize(request, SerializerOptions);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"OpenAI chat completions API returned {(int)response.StatusCode} {response.StatusCode}: {errorContent}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var completionResponse = await JsonSerializer.DeserializeAsync<OpenAiChatCompletionResponse>(
            stream,
            SerializerOptions,
            cancellationToken);

        if (completionResponse is null)
        {
            throw new InvalidOperationException("OpenAI response body was empty.");
        }

        return completionResponse;
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || _options.ApiKey == "YOUR_OPENAI_API_KEY")
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }
    }
}
