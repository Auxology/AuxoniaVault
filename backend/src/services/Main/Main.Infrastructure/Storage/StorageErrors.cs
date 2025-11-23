using Main.SharedKernel;

namespace Main.Infrastructure.Storage;

internal static class StorageErrors
{
    public static Error UploadFailed => Error.Failure
    (
        "Storage.UploadFailed",
        "An error occurred while uploading the file to storage."
    );
    
    public static Error UnexpectedError => Error.Failure
    (
        "Storage.UnexpectedError",
        "An unexpected error occurred in the storage service."
    );
    
    public static Error DeletionFailed => Error.Failure
    (
        "Storage.DeletionFailed",
        "An error occurred while deleting the file from storage."
    );
    
    public static Error DownloadFailed => Error.Failure
    (
        "Storage.DownloadFailed",
        "An error occurred while downloading the file from storage."
    );
}