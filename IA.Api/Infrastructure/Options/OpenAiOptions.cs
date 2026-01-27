using System.Collections.Generic;

namespace IA.Api.Infrastructure.Options;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
    public string ApiKey { get; set; } = string.Empty;
    public string ResponsesModel { get; set; } = "o4-mini";
    public string SystemPrompt { get; set; } = "Responda sempre em português de forma curta e objetiva.";
    public List<string> VectorStoreIds { get; set; } = new();
    public string WorkflowId { get; set; } = string.Empty;
}
