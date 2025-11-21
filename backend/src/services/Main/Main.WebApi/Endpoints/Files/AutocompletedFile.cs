using Main.Application.Files.AutocompleteFiles;
using Main.WebApi.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.WebApi.Endpoints.Files;

internal sealed class AutocompleteFiles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/files/autocomplete", async
        (
            [FromQuery] string q,
            [FromQuery] int limit,
            HttpContext httpContext,
            ISender sender
        ) =>
        {
            var query = new AutocompleteFilesQuery(
                SearchTerm: q,
                Limit: limit <= 0 ? 10 : limit
            );

            var result = await sender.Send(query);

            return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, httpContext);
        })
        .RequireAuthorization()
        .WithName("AutocompleteFiles")
        .WithTags(Tags.Files)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get filename autocomplete suggestions";
            operation.Description = "Returns a list of filename suggestions based on the search term for autocomplete functionality. " +
                                    "Query parameter 'q' is required (search term), 'limit' is optional (default: 10, max: 50)";
            return operation;
        });
    }
}