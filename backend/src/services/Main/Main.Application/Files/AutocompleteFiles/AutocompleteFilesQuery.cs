using Main.Application.Abstractions.Messaging;

namespace Main.Application.Files.AutocompleteFiles;

public sealed record AutocompleteFilesQuery(
    string SearchTerm,
    int Limit
) : IQuery<AutocompleteFilesResponse>;