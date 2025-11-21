using FluentValidation;
using Main.Domain.Constants;

namespace Main.Application.Files.AutocompleteFiles;

internal sealed class AutocompleteFilesQueryValidator : AbstractValidator<AutocompleteFilesQuery>
{
    public AutocompleteFilesQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .WithMessage("Search term is required")
            .MinimumLength(FileConstants.MinSearchTermLength)
            .WithMessage("Search term must be at least 1 character")
            .MaximumLength(FileConstants.MaxSearchTermLength)
            .WithMessage("Search term cannot exceed 100 characters");

        RuleFor(x => x.Limit)
            .InclusiveBetween(FileConstants.MinAutocompleteLimit, FileConstants.MaxAutocompleteLimit)
            .WithMessage("Limit must be between 1 and 50");
    }
}