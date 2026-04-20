namespace IA.Api.Domain.Models;

public sealed record AudioTranscriptionCommand(
    Stream AudioStream,
    string FileName,
    string? ContentType,
    long Length);
