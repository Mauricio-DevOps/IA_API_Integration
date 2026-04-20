using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace IA.Api.Presentation.Requests;

public sealed class CreateAudioTranscriptionRequest
{
    [Required]
    public IFormFile AudioFile { get; init; } = default!;
}
