using Main.SharedKernel;

namespace Main.Application.Errors;

internal static class FileMetadataErrors
{
    public static Error NotFound => Error.NotFound
    (
        code: "Files.NotFound",
        description: "The specified file was not found."
    );
}