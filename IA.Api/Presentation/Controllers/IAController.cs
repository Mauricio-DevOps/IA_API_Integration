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
    private static readonly HashSet<string> SupportedAudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ogg",
        ".webm"
    };

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
        try
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
        catch (InvalidOperationException ex)
        {
            return PlainTextError(StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return PlainTextError(StatusCodes.Status502BadGateway, ex.Message);
        }
        catch (Exception ex)
        {
            return PlainTextError(StatusCodes.Status500InternalServerError, ex.Message);
        }
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

    [HttpPost("ask/vector-store")]
    [ProducesResponseType(typeof(AssistantResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssistantResponse>> AskAssistantWithVectorStore(
        [FromBody] AskIaWithVectorStoreRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ResponsesCommand(
                request.Prompt,
                request.PreviousResponseId,
                useFileSearch: true,
                vectorStoreIds: new[] { request.VectorStoreId });

            var response = await _responsesService.CreateResponseAsync(command, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return PlainTextError(StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return PlainTextError(StatusCodes.Status502BadGateway, ex.Message);
        }
        catch (Exception ex)
        {
            return PlainTextError(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost("prompts/{promptId}/messages")]
    [ProducesResponseType(typeof(PromptConversationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PromptConversationResponse>> SendPromptMessage(
        [FromRoute] string promptId,
        [FromBody] PromptConversationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(promptId))
        {
            return PlainTextError(StatusCodes.Status400BadRequest, "promptId is required.");
        }

        try
        {
            var command = new PromptConversationCommand(
                promptId,
                request.Message,
                request.ConversationId);

            var response = await _responsesService.CreatePromptResponseAsync(command, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return PlainTextError(StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return PlainTextError(StatusCodes.Status502BadGateway, ex.Message);
        }
        catch (Exception ex)
        {
            return PlainTextError(StatusCodes.Status500InternalServerError, ex.Message);
        }
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
            return PlainTextError(StatusCodes.Status400BadRequest, "AudioFile cannot be empty.");
        }

        await using var audioStream = request.AudioFile.OpenReadStream();
        var fileName = BuildTranscriptionFileName(request.AudioFile.FileName);
        if (fileName is null)
        {
            return PlainTextError(
                StatusCodes.Status400BadRequest,
                "AudioFile must have a .ogg or .webm extension.");
        }

        try
        {
            var command = new AudioTranscriptionCommand(
                audioStream,
                fileName,
                request.AudioFile.ContentType,
                request.AudioFile.Length);

            var transcript = await _transcriptionService.TranscribeAsync(command, cancellationToken);
            return Content(transcript, "text/plain");
        }
        catch (InvalidOperationException ex)
        {
            return PlainTextError(StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return PlainTextError(StatusCodes.Status502BadGateway, ex.Message);
        }
        catch (Exception ex)
        {
            return PlainTextError(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    private static string? BuildTranscriptionFileName(string originalFileName)
    {
        var safeBaseName = Path.GetFileNameWithoutExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(safeBaseName))
        {
            safeBaseName = "audio";
        }

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension) || !SupportedAudioExtensions.Contains(extension))
        {
            return null;
        }

        return $"{safeBaseName}{extension}";
    }

    private static ContentResult PlainTextError(int statusCode, string message)
    {
        return new ContentResult
        {
            StatusCode = statusCode,
            ContentType = "text/plain",
            Content = message
        };
    }
}
