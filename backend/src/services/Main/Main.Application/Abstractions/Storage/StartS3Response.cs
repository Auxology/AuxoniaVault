namespace Main.Application.Abstractions.Storage;

public sealed record StartS3Response
(
    string FileKey,
    string UploadId
);