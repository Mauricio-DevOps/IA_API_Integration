using System.Text.Json.Serialization;

namespace IA.Api.Domain.Models;

public sealed class OpenAiChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; }

    [JsonPropertyName("messages")]
    public IReadOnlyList<OpenAiChatMessage> Messages { get; }

    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Temperature { get; }

    public OpenAiChatCompletionRequest(
        string model,
        IReadOnlyList<OpenAiChatMessage> messages,
        double? temperature = null)
    {
        Model = model;
        Messages = messages;
        Temperature = temperature;
    }
}
