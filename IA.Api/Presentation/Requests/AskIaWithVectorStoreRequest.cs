using System.ComponentModel.DataAnnotations;

namespace IA.Api.Presentation.Requests;

public sealed class AskIaWithVectorStoreRequest
{
    [Required]
    public string Prompt { get; init; } = string.Empty;

    public string? PreviousResponseId { get; init; }

    [Required]
    public string VectorStoreId { get; init; } = string.Empty;
}
