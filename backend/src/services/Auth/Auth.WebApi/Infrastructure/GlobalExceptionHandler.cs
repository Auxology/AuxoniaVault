using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Auth.WebApi.Infrastructure;

internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        
        Activity? activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        
        var title = exception switch
        {
            ValidationException => "One or more validation errors occurred.",
            _ => "An unexpected error occurred."
        };
        
        var type = exception switch
        {
            ValidationException => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = type,
                Title = title,
                Detail = exception.Message,
                Instance = $"{httpContext.Request.Method}:{httpContext.Request.Path}",
                Status = httpContext.Response.StatusCode,
                Extensions = new Dictionary<string, object?>
                {
                    ["requestId"] = httpContext.TraceIdentifier,
                    ["traceId"] = activity?.Id
                }
            }
        });
    }
}