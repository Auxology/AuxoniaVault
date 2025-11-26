using FluentValidation;

namespace Main.Application.Files.DownloadMultipleFiles;

internal sealed class DownloadMultipleFilesCommandValidator : AbstractValidator<DownloadMultipleFilesCommand>
{
    public DownloadMultipleFilesCommandValidator()
    {
        RuleFor(x => x.FileIds)
            .NotNull()
            .WithMessage("File IDs are required")
            .NotEmpty()
            .WithMessage("At least one file ID must be provided")
            .Must(ids => ids.Count <= 50)
            .WithMessage("Cannot download more than 50 files at once");
    }
}