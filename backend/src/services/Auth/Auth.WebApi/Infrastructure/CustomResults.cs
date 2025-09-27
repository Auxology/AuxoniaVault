using System.Diagnostics;
using Auth.SharedKernel;
using Microsoft.AspNetCore.Http.Features;

namespace Auth.WebApi.Infrastructure;

public static class CustomResults
{
    public static IResult Problem(Result result, HttpContext httpContext)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot create a problem result from a successful result.");
        
        return Results.Problem(
            type: GetType(result.Error.Type),
            title: GetTitle(result.Error),
            detail: GetDetail(result.Error),
            instance: GetInstance(httpContext),
            statusCode: GetStatusCode(result.Error.Type),
            extensions: GetExtensions(httpContext));
        
        static string GetType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };
        
        static string GetTitle(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => error.Code,
                ErrorType.Problem => error.Code,
                ErrorType.NotFound => error.Code,
                ErrorType.Conflict => error.Code,
                ErrorType.Unauthorized => error.Code,
                _ => "Server failure"
            };
        
        static string GetDetail(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => error.Description,
                ErrorType.Problem => error.Description,
                ErrorType.NotFound => error.Description,
                ErrorType.Conflict => error.Description,
                ErrorType.Unauthorized => error.Description,
                _ => "An unexpected error occurred"
            };
        
        static string GetInstance(HttpContext httpContext) =>
            $"{httpContext.Request.Method}:{httpContext.Request.Path}";
        
        
        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation or ErrorType.Problem => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

        static Dictionary<string, object?> GetExtensions(HttpContext httpContext)
        {
            Activity? activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity;
            
            return new Dictionary<string, object?>
            {
                ["requestId"] = httpContext.TraceIdentifier,
                ["traceId"] = activity?.Id
            };
        }
    }
}