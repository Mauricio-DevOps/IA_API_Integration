namespace IA.Api.Domain.Models;

public sealed record GeneratedImage(
    string? B64Json,
    string? Url,
    string? RevisedPrompt,
    string MimeType);
