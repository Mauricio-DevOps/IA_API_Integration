namespace IA.Api.Domain.Models;

public sealed class ResponsesCommand
{
    public ResponsesCommand(
        string prompt,
        string? previousResponseId = null,
        bool useFileSearch = false,
        IReadOnlyList<string>? vectorStoreIds = null)
    {
        Prompt = prompt;
        PreviousResponseId = previousResponseId;
        UseFileSearch = useFileSearch;
        VectorStoreIds = vectorStoreIds;
    }

    public string Prompt { get; }
    public string? PreviousResponseId { get; }
    public bool UseFileSearch { get; }
    public IReadOnlyList<string>? VectorStoreIds { get; }
}
