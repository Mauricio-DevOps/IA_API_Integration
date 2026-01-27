namespace IA.Api.Domain.Models;

public sealed record WorkflowSessionResponse(
    string SessionId,
    string ClientSecret,
    long? ExpiresAt);
