namespace IA.Api.Domain.Models;

public sealed class PromptConversationCommand
{
    public PromptConversationCommand(
        string promptId,
        string message,
        string? conversationId = null)
    {
        PromptId = promptId;
        Message = message;
        ConversationId = conversationId;
    }

    public string PromptId { get; }
    public string Message { get; }
    public string? ConversationId { get; }
}
