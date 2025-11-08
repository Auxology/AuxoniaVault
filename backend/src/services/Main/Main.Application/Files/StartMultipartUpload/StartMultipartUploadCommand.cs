using Main.Application.Abstractions.Messaging;

namespace Main.Application.Files.StartMultipartUpload;

public sealed record StartMultipartUploadCommand
(
    string FileName,
    long FileSize,
    string ContentType
) : ICommand<StartMultipartUploadResponse>;

public sealed record StartMultipartUploadResponse
(
    string FileKey,
    string UploadId
);