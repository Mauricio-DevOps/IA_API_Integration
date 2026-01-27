using IA.Api.Domain.Models;

namespace IA.Api.Application.Contracts;

public interface IOpenAiService
{
    Task<OpenAiChatCompletionResponse> CreateChatCompletionAsync(
        OpenAiChatCompletionRequest request,
        CancellationToken cancellationToken = default);
}
