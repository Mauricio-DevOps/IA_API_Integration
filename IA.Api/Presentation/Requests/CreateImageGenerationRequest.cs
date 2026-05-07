using System.ComponentModel.DataAnnotations;

namespace IA.Api.Presentation.Requests;

public sealed class CreateImageGenerationRequest
{
    [Required]
    public string Prompt { get; init; } = string.Empty;

    [Required]
    public string Model { get; init; } = string.Empty;

    public string? Size { get; init; }

    public string? Quality { get; init; }

    public string? Background { get; init; }

    public string? OutputFormat { get; init; }

    [Range(1, 10)]
    public int? N { get; init; }
}
