using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IA.Api.Application.Contracts;
using IA.Api.Domain.Models;
using IA.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace IA.Api.Infrastructure.OpenAI;

public sealed class OpenAiImageService : IOpenAiImageService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiImageService(HttpClient httpClient, IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }
    }

    public async Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationCommand command,
        CancellationToken cancellationToken = default)
    {
        EnsureApiKey();
        ValidateInput(command);

        var payload = SerializePayload(command);
        using var request = new HttpRequestMessage(HttpMethod.Post, "images/generations")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"OpenAI image generation API returned {(int)response.StatusCode} {response.StatusCode}: {errorContent}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return DeserializeResponse(document.RootElement, command.OutputFormat);
    }

    private static string SerializePayload(ImageGenerationCommand command)
    {
        var body = new Dictionary<string, object?>
        {
            ["model"] = command.Model,
            ["prompt"] = command.Prompt
        };

        if (!string.IsNullOrWhiteSpace(command.Size))
        {
            body["size"] = command.Size;
        }

        if (!string.IsNullOrWhiteSpace(command.Quality))
        {
            body["quality"] = command.Quality;
        }

        if (!string.IsNullOrWhiteSpace(command.Background))
        {
            body["background"] = command.Background;
        }

        if (!string.IsNullOrWhiteSpace(command.OutputFormat))
        {
            body["output_format"] = command.OutputFormat;
        }

        if (command.N.HasValue)
        {
            body["n"] = command.N.Value;
        }

        return JsonSerializer.Serialize(body, SerializerOptions);
    }

    private static ImageGenerationResponse DeserializeResponse(JsonElement root, string? outputFormat)
    {
        long? created = null;
        if (root.TryGetProperty("created", out var createdElement)
            && createdElement.ValueKind == JsonValueKind.Number)
        {
            created = createdElement.GetInt64();
        }

        if (!root.TryGetProperty("data", out var dataElement)
            || dataElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("OpenAI image generation response did not include image data.");
        }

        var images = new List<GeneratedImage>();
        foreach (var item in dataElement.EnumerateArray())
        {
            var b64Json = TryGetString(item, "b64_json");
            var url = TryGetString(item, "url");
            var revisedPrompt = TryGetString(item, "revised_prompt");

            images.Add(new GeneratedImage(
                b64Json,
                url,
                revisedPrompt,
                ResolveMimeType(outputFormat)));
        }

        if (images.Count == 0)
        {
            throw new InvalidOperationException("OpenAI image generation response did not include images.");
        }

        return new ImageGenerationResponse(images, created);
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
            ? property.GetString()
            : null;
    }

    private static string ResolveMimeType(string? outputFormat)
    {
        return outputFormat?.Trim().ToLowerInvariant() switch
        {
            "jpeg" or "jpg" => "image/jpeg",
            "webp" => "image/webp",
            _ => "image/png"
        };
    }

    private static void ValidateInput(ImageGenerationCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Prompt))
        {
            throw new InvalidOperationException("Prompt is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Model))
        {
            throw new InvalidOperationException("Model is required.");
        }

        if (command.N is <= 0 or > 10)
        {
            throw new InvalidOperationException("N must be between 1 and 10.");
        }
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || _options.ApiKey == "YOUR_OPENAI_API_KEY")
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }
    }
}
