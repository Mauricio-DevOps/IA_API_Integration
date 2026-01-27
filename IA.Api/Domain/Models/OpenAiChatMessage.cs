using System.Text.Json.Serialization;

namespace IA.Api.Domain.Models;

public sealed record OpenAiChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);
