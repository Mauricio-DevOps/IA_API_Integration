using IA.Api.Domain.Models;

namespace IA.Api.Application.Contracts;

public interface IOpenAiResponsesService
{
    Task<AssistantResponse> CreateResponseAsync(
        ResponsesCommand command,
        CancellationToken cancellationToken = default);
}
