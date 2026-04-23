using System.ComponentModel.DataAnnotations;

namespace IA.Api.Presentation.Requests;

public sealed class PromptConversationRequest
{
    [Required]
    public string Message { get; init; } = string.Empty;

    public string? ConversationId { get; init; }
}
