using Main.Application.Abstractions.Messaging;

namespace Main.Application.Files.DownloadFile;

public sealed record DownloadFileCommand
(
    Guid FileId
) : ICommand<string>;