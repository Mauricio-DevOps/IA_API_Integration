using System.Text.Json;
using System.Text.Json.Serialization;

namespace IA.Api.Domain.Models;

public sealed record AssistantResponse(
    string ResponseId,
    string OutputText)
{
    [JsonIgnore]
    public JsonElement Raw { get; init; }

}
