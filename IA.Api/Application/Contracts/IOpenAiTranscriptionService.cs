using IA.Api.Domain.Models;

namespace IA.Api.Application.Contracts;

public interface IOpenAiTranscriptionService
{
    Task<string> TranscribeAsync(
        AudioTranscriptionCommand command,
        CancellationToken cancellationToken = default);
}
