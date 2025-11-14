using Gateway.Application.Services;
using Gateway.SharedKernel;
using Gateway.WebApi.Extensions;
using Gateway.WebApi.Infrastructure;
using Shared.Requests;

namespace Gateway.WebApi.Endpoints.Auth;

internal sealed class VerifyLogin : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("bff/auth/verify-login", async
        (
            HttpContext httpContext,
            VerifyLoginRequest request,
            IBffAuthService bffAuthService,
            CancellationToken cancellationToken
        ) =>
        {
            Result<string> result = await bffAuthService.VerifyLoginHandlerAsync(request, cancellationToken);
            
            if (result.IsFailure) 
                return CustomResults.Problem(result, httpContext);
            
            httpContext.SetAuthenticationCookie(result.Value);
            
            return Results.Ok();
        })
        .WithTags(Tags.Auth)
        .WithName("VerifyLogin")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Verify user login and set authentication cookie";
            operation.Description = "Verifies the user's login credentials and sets an authentication cookie upon successful verification.";
            return operation;
        });
    }
}