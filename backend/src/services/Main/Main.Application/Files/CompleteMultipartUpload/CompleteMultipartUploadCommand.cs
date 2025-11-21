using Main.Application.Abstractions.Messaging;
using Main.Domain.Objects;

namespace Main.Application.Files.CompleteMultipartUpload;

public sealed record CompleteMultipartUploadCommand
(
    string FileKey,
    string UploadId,
    List<PartETag> Parts,
    string FileName,
    long FileSizeInBytes,
    string ContentType
) : ICommand<string>;
