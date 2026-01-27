using System.Text.Json.Serialization;

namespace IA.Api.Domain.Models;

public sealed record OpenAiChatCompletionResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("created")] long Created,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("choices")] IReadOnlyList<OpenAiChatCompletionChoice> Choices,
    [property: JsonPropertyName("usage")] OpenAiUsage Usage);
