using Main.Application.Abstractions.Messaging;

namespace Main.Application.Files.DownloadMultipleFiles;

public sealed record DownloadMultipleFilesCommand
(
    List<Guid> FileIds
) : ICommand<DownloadMultipleFilesResponse>;