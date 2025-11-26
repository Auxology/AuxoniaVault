using Main.SharedKernel;

namespace Main.Application.Errors;

internal static class FileMetadataErrors
{
    public static Error NotFound => Error.NotFound
    (
        code: "Files.NotFound",
        description: "The specified file was not found."
    );
    
    public static Error SomeFilesNotFound => Error.NotFound
    (
        code: "Files.SomeFilesNotFound",
        description: "Some of the specified files were not found, please verify the file IDs and try again."
    );
}