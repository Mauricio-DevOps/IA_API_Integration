namespace IA.Api.Domain.Models;

public sealed record PromptConversationResponse(
    string ResponseId,
    string ConversationId,
    string OutputText);
