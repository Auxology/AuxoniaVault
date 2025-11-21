using FluentValidation;
using Main.Domain.Constants;

namespace Main.Application.Files.SearchFiles;

internal sealed class SearchFilesQueryValidator : AbstractValidator<SearchFilesQuery>
{
    public SearchFilesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(FileConstants.MinPageSize, FileConstants.MaxPageSize)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.SearchTerm))
            .WithMessage("Search term cannot exceed 500 characters");

        RuleFor(x => x.MinSizeInBytes)
            .GreaterThanOrEqualTo(FileConstants.MinAllowedFileSizeInBytes)
            .When(x => x.MinSizeInBytes.HasValue)
            .WithMessage("MinFileSize must be greater than or equal to 0");

        RuleFor(x => x.MaxSizeInBytes)
            .GreaterThanOrEqualTo(FileConstants.MinAllowedFileSizeInBytes)
            .When(x => x.MaxSizeInBytes.HasValue)
            .WithMessage("MaxFileSize must be greater than or equal to 0");

        RuleFor(x => x)
            .Must(x => !x.MinSizeInBytes.HasValue || !x.MaxSizeInBytes.HasValue || x.MinSizeInBytes.Value <= x.MaxSizeInBytes.Value)
            .WithMessage("MinFileSize must be less than or equal to MaxFileSize");

        RuleFor(x => x)
            .Must(x => !x.CreatedAfter.HasValue || !x.CreatedBefore.HasValue || x.CreatedAfter.Value <= x.CreatedBefore.Value)
            .WithMessage("CreatedAfter must be less than or equal to CreatedBefore");

        RuleFor(x => x.SortBy)
            .NotEmpty()
            .WithMessage("SortBy is required")
            .Must(sortBy => new[] { "relevance", "name", "name_desc", "date", "date_desc", "size", "size_desc" }
                .Contains(sortBy.ToLowerInvariant()))
            .WithMessage("Invalid sort option");
    }
}