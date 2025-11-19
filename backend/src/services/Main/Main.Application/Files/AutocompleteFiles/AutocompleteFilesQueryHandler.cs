using Main.Application.Abstractions.Authentication;
using Main.Application.Abstractions.Database;
using Main.Application.Abstractions.Messaging;
using Main.Application.Errors;
using Main.Domain.Aggregates.Account;
using Main.Domain.ValueObjects;
using Main.Infrastructure.Search.Services;
using Main.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Files.AutocompleteFiles;

internal sealed class AutocompleteFilesQueryHandler
(
    IMainDbContext context,
    IUserContext userContext,
    IOpenSearchService searchService
)
    : IQueryHandler<AutocompleteFilesQuery, AutocompleteFilesResponse>
{
    public async Task<Result<AutocompleteFilesResponse>> Handle(
        AutocompleteFilesQuery request, 
        CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);
        
        Account? account = await context.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, cancellationToken);
        
        if (account is null)
            return Result.Failure<AutocompleteFilesResponse>(AccountErrors.NotFound);
        
        Result<IReadOnlyList<string>> result = await searchService.AutocompleteAsync
        (
            searchTerm: request.SearchTerm,
            ownerId: userId.Value,
            limit: request.Limit,
            cancellationToken: cancellationToken
        );

        if (result.IsFailure)
            return Result.Failure<AutocompleteFilesResponse>(result.Error);

        var response = new AutocompleteFilesResponse(result.Value);
        
        return Result.Success(response);
    }
}