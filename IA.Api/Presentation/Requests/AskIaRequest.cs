using System.ComponentModel.DataAnnotations;

namespace IA.Api.Presentation.Requests;

public sealed class AskIaRequest
{
    [Required]
    public string Prompt { get; init; } = string.Empty;

    public string? PreviousResponseId { get; init; }

    public bool UseFileSearch { get; init; }
}
