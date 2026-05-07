namespace IA.Api.Domain.Models;

public sealed record ImageGenerationResponse(
    IReadOnlyList<GeneratedImage> Images,
    long? Created);
