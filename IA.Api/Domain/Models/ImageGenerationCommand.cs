namespace IA.Api.Domain.Models;

public sealed record ImageGenerationCommand(
    string Prompt,
    string Model,
    string? Size,
    string? Quality,
    string? Background,
    string? OutputFormat,
    int? N);
