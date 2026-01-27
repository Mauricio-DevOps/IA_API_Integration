using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IA.Api.Presentation.Requests;

public sealed class CreateWorkflowSessionRequest
{
    [Required]
    public string User { get; init; } = string.Empty;

    public Dictionary<string, object?>? StateVariables { get; init; }

    public string? WorkflowVersion { get; init; }
}
