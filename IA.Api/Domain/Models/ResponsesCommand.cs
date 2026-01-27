namespace IA.Api.Domain.Models;

public sealed class ResponsesCommand
{
    public ResponsesCommand(
        string prompt,
        string? previousResponseId = null,
        bool useFileSearch = false)
    {
        Prompt = prompt;
        PreviousResponseId = previousResponseId;
        UseFileSearch = useFileSearch;
    }

    public string Prompt { get; }
    public string? PreviousResponseId { get; }
    public bool UseFileSearch { get; }
}
