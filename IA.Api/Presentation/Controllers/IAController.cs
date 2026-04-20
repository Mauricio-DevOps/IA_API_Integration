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
    private readonly IOpenAiTranscriptionService _transcriptionService;
    private readonly IOpenAiWorkflowService _workflowService;

    public IAController(
        IOpenAiService openAiService,
        IOpenAiResponsesService responsesService,
        IOpenAiTranscriptionService transcriptionService,
        IOpenAiWorkflowService workflowService)
    {
        _openAiService = openAiService;
        _responsesService = responsesService;
        _transcriptionService = transcriptionService;
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

    [HttpPost("transcribe")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> TranscribeAudio(
        [FromForm] CreateAudioTranscriptionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.AudioFile.Length <= 0)
        {
            return BadRequest("AudioFile cannot be empty.");
        }

        await using var audioStream = request.AudioFile.OpenReadStream();
        var fileName = BuildTranscriptionFileName(request.AudioFile.FileName);
        var command = new AudioTranscriptionCommand(
            audioStream,
            fileName,
            request.AudioFile.ContentType,
            request.AudioFile.Length);

        var transcript = await _transcriptionService.TranscribeAsync(command, cancellationToken);
        return Content(transcript, "text/plain");
    }

    private static string BuildTranscriptionFileName(string originalFileName)
    {
        var safeBaseName = Path.GetFileNameWithoutExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(safeBaseName))
        {
            safeBaseName = "audio";
        }

        return $"{safeBaseName}.ogg";
    }
}
