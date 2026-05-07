using IA.Api.Domain.Models;

namespace IA.Api.Application.Contracts;

public interface IOpenAiImageService
{
    Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationCommand command,
        CancellationToken cancellationToken = default);
}
