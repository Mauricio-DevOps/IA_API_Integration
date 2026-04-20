using System.Net.Http.Headers;
using IA.Api.Application.Contracts;
using IA.Api.Domain.Models;
using IA.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace IA.Api.Infrastructure.OpenAI;

public sealed class OpenAiTranscriptionService : IOpenAiTranscriptionService
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".mp4",
        ".mpeg",
        ".mpga",
        ".m4a",
        ".ogg",
        ".wav",
        ".webm"
    };

    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiTranscriptionService(HttpClient httpClient, IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }
    }

    public async Task<string> TranscribeAsync(
        AudioTranscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        EnsureConfiguration();
        ValidateInput(command);

        using var request = new HttpRequestMessage(HttpMethod.Post, "audio/transcriptions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(new StringContent(_options.TranscriptionModel), "model");
        multipartContent.Add(new StringContent("text"), "response_format");

        using var fileContent = new StreamContent(command.AudioStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(command.ContentType ?? "application/octet-stream");
        multipartContent.Add(fileContent, "file", command.FileName);

        request.Content = multipartContent;

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"OpenAI audio transcription API returned {(int)response.StatusCode} {response.StatusCode}: {responseBody}");
        }

        return responseBody.Trim();
    }

    private static void ValidateInput(AudioTranscriptionCommand command)
    {
        if (command.AudioStream is null)
        {
            throw new ArgumentNullException(nameof(command.AudioStream));
        }

        if (string.IsNullOrWhiteSpace(command.FileName))
        {
            throw new InvalidOperationException("Audio file name is required.");
        }

        if (command.Length <= 0)
        {
            throw new InvalidOperationException("Audio file cannot be empty.");
        }

        if (command.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("Audio file exceeds the 25 MB limit supported by OpenAI.");
        }

        var extension = Path.GetExtension(command.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !SupportedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"Unsupported audio extension '{extension}'. Supported extensions: {string.Join(", ", SupportedExtensions)}.");
        }

        var contentType = command.ContentType;
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            var normalizedContentType = contentType.Split(';', 2, StringSplitOptions.TrimEntries)[0];
            if (!normalizedContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
                && !normalizedContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
                && normalizedContentType != "application/octet-stream")
            {
                throw new InvalidOperationException($"Unsupported audio content type '{command.ContentType}'.");
            }
        }
    }

    private void EnsureConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.TranscriptionModel))
        {
            throw new InvalidOperationException("OpenAI transcription model is not configured.");
        }
    }
}
