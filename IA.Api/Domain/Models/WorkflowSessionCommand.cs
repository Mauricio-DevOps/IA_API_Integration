using System.Collections.Generic;

namespace IA.Api.Domain.Models;

public sealed record WorkflowSessionCommand(
    string User,
    Dictionary<string, object?>? StateVariables,
    string? WorkflowVersion);
