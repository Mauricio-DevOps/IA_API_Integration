using IA.Api.Domain.Models;

namespace IA.Api.Application.Contracts;

public interface IOpenAiWorkflowService
{
    Task<WorkflowSessionResponse> CreateSessionAsync(
        WorkflowSessionCommand command,
        CancellationToken cancellationToken = default);
}
