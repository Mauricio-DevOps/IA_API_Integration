using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IA.Api.Application.Contracts;
using IA.Api.Domain.Models;
using IA.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace IA.Api.Infrastructure.OpenAI;

public sealed class OpenAiWorkflowService : IOpenAiWorkflowService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiWorkflowService(HttpClient httpClient, IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }
    }

    public async Task<WorkflowSessionResponse> CreateSessionAsync(
        WorkflowSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        EnsureConfiguration();

        using var request = new HttpRequestMessage(HttpMethod.Post, "chatkit/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.Add("OpenAI-Beta", "chatkit_beta=v1");

        var workflowPayload = new Dictionary<string, object?>
        {
            ["id"] = _options.WorkflowId,
            ["state_variables"] = command.StateVariables ?? new Dictionary<string, object?>()
        };

        if (!string.IsNullOrWhiteSpace(command.WorkflowVersion))
        {
            workflowPayload["version"] = command.WorkflowVersion;
        }

        var payload = new Dictionary<string, object?>
        {
            ["user"] = command.User,
            ["workflow"] = workflowPayload
        };

        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"OpenAI ChatKit session API returned {(int)response.StatusCode} {response.StatusCode}: {responseBody}");
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        var sessionId = root.GetProperty("id").GetString() ?? string.Empty;
        var clientSecret = root.GetProperty("client_secret").GetString() ?? string.Empty;
        long? expiresAt = null;

        if (root.TryGetProperty("expires_at", out var expiresElement)
            && expiresElement.ValueKind == JsonValueKind.Number)
        {
            expiresAt = expiresElement.GetInt64();
        }

        return new WorkflowSessionResponse(sessionId, clientSecret, expiresAt);
    }

    private void EnsureConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.WorkflowId))
        {
            throw new InvalidOperationException("OpenAI workflow id is not configured.");
        }
    }
}
