using Main.SharedKernel;

namespace Main.Infrastructure.Search.Errors;

internal static class SearchErrors
{
    public static readonly Error IndexCreationFailed = Error.Failure
    (
        code: "Search.IndexCreationFailed",
        description: "Failed to create search index"
    );

    public static readonly Error IndexingFailed = Error.Failure
    (
        code: "Search.IndexingFailed",
        description: "Failed to index document"
    );

    public static readonly Error DeletionFailed = Error.Failure
    (
        code: "Search.DeletionFailed",
        description: "Failed to delete document from search index"
    );

    public static readonly Error SearchFailed = Error.Failure
    (
        code: "Search.SearchFailed",
        description: "Failed to perform search"
    );

    public static readonly Error UnexpectedError = Error.Failure
    (
        code: "Search.UnexpectedError",
        description: "An unexpected error occurred during search operation"
    );
}