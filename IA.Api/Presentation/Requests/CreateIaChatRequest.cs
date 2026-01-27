using System.ComponentModel.DataAnnotations;

namespace IA.Api.Presentation.Requests;

public sealed class CreateIaChatRequest
{
    [Required]
    public string Prompt { get; init; } = string.Empty;

    public string? SystemPrompt { get; init; }

    public string? Model { get; init; }

    [Range(0, 2)]
    public double? Temperature { get; init; }
}
