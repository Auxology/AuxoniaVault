using Main.Application.Abstractions.Messaging;

namespace Main.Application.Files.DeleteFile;

public sealed record DeleteFileCommand
(
    Guid FileId
) : ICommand;