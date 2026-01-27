using System.Text.Json.Serialization;

namespace IA.Api.Domain.Models;

public sealed record OpenAiChatCompletionChoice(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("message")] OpenAiChatMessage Message,
    [property: JsonPropertyName("finish_reason")] string FinishReason);
