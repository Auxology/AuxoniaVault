using Auth.WebApi.Extensions;

namespace Auth.WebApi.Infrastructure;

internal sealed class RequireAuthenticationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        var userIdHeader = httpContext.GetUserIdFromGateway();
        var emailHeader = httpContext.GetUserEmailFromGateway();

        if (string.IsNullOrEmpty(userIdHeader) || !Guid.TryParse(userIdHeader, out _))
            return Results.Unauthorized();
        

        if (string.IsNullOrEmpty(emailHeader))
            return Results.Unauthorized();
        
        return await next(context);
    }
}