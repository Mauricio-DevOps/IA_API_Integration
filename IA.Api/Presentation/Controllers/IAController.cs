using IA.Api.Application.Contracts;
using IA.Api.Domain.Models;
using IA.Api.Presentation.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IA.Api.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IAController : ControllerBase
{
    private const string DefaultModel = "gpt-4o-mini";
    private const double DefaultTemperature = 0.7d;
    private const string DefaultSystemPrompt = "You are a helpful AI assistant.";

    private readonly IOpenAiService _openAiService;
    private readonly IOpenAiResponsesService _responsesService;
    private readonly IOpenAiWorkflowService _workflowService;

    public IAController(
        IOpenAiService openAiService,
        IOpenAiResponsesService responsesService,
        IOpenAiWorkflowService workflowService)
    {
        _openAiService = openAiService;
        _responsesService = responsesService;
        _workflowService = workflowService;
    }

    [HttpPost("chat")]
    [ProducesResponseType(typeof(OpenAiChatCompletionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OpenAiChatCompletionResponse>> CreateChatCompletion(
        [FromBody] CreateIaChatRequest request,
        CancellationToken cancellationToken)
    {
        var systemPrompt = string.IsNullOrWhiteSpace(request.SystemPrompt)
            ? DefaultSystemPrompt
            : request.SystemPrompt!;

        var messages = new List<OpenAiChatMessage>
        {
            new("system", systemPrompt),
            new("user", request.Prompt)
        };

        var completionRequest = new OpenAiChatCompletionRequest(
            request.Model ?? DefaultModel,
            messages,
            request.Temperature ?? DefaultTemperature);

        var response = await _openAiService.CreateChatCompletionAsync(completionRequest, cancellationToken);

        return Ok(response);
    }

    [HttpPost("ask")]
    [ProducesResponseType(typeof(AssistantResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssistantResponse>> AskAssistant(
        [FromBody] AskIaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResponsesCommand(
            request.Prompt,
            request.PreviousResponseId,
            request.UseFileSearch);

        var response = await _responsesService.CreateResponseAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("workflow/session")]
    [ProducesResponseType(typeof(WorkflowSessionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkflowSessionResponse>> CreateWorkflowSession(
        [FromBody] CreateWorkflowSessionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new WorkflowSessionCommand(
            request.User,
            request.StateVariables,
            request.WorkflowVersion);

        var response = await _workflowService.CreateSessionAsync(command, cancellationToken);
        return Ok(response);
    }
}
