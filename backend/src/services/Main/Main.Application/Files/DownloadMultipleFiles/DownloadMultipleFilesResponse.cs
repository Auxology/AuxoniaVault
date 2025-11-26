namespace Main.Application.Files.DownloadMultipleFiles;

public sealed record DownloadMultipleFilesResponse
(
    IReadOnlyList<FileDownloadResult> Results,
    int SuccessCount,
    int FailureCount
);

public sealed record FileDownloadResult
(
    Guid FileId,
    string? DownloadUrl,
    string? FileName,
    bool IsSuccess,
    string? ErrorMessage
);