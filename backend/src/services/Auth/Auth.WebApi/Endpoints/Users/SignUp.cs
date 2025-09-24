using Auth.Application.Users;
using Auth.Application.Users.SignUp;
using Auth.SharedKernel;
using Auth.WebApi.Infrastructure;
using MediatR;

namespace Auth.WebApi.Endpoints.Users;

internal sealed class SignUp : IEndpoint
{
    private sealed record Request(string Name, string Email);
    
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/sign-up", async
        (
            Request request,
            HttpContext httpContext,
            ISender sender
        ) =>
        {
            var command = new SignUpCommand(request.Name, request.Email);
            
            Result<Guid> result = await sender.Send(command);
            
            return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result, httpContext);
        })
        .WithTags(Tags.Users)
        .AllowAnonymous();
    }
}