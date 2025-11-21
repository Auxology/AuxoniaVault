namespace Main.Application.Files.AutocompleteFiles;

public sealed record AutocompleteFilesResponse
(
    IReadOnlyList<string> Suggestions
);