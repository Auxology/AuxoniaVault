using Main.SharedKernel;

namespace Main.Domain.Errors;

internal static class FileMetadataErrors
{
    public static Error UserIdRequired => Error.Validation
    (
        code: "Files.UserIdRequired",
        description: "UserId is required."
    );
    
    public static Error NameRequired => Error.Validation
    (
        code: "Files.NameRequired",
        description: "File name is required."
    );
    
    public static Error ContentTypeRequired => Error.Validation
    (
        code: "Files.ContentTypeRequired",
        description: "Content type is required."
    );
    
    public static Error InvalidFileSize => Error.Validation
    (
        code: "Files.InvalidFileSize",
        description: "File size must be greater than zero."
    );
    
    public static Error FileKeyRequired => Error.Validation
    (
        code: "Files.FileKeyRequired",
        description: "File key is required."
    );
    
    public static Error FileNameTooLong => Error.Validation
    (
        code: "Files.FileNameTooLong",
        description: "File name is too long."
    );
}