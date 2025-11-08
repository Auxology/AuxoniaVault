using Main.Application.Abstractions.Messaging;

namespace Main.Application.Files.PresignedPartUpload;

public sealed record class PresignedPartUploadCommand
(
    string FileKey,
    string UploadId,
    int PartNumber
) : ICommand<PresignedPartUploadResponse>;

public sealed record PresignedPartUploadResponse 
(
    string PresignedUrl,
    int PartNumber
);