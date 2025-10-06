using Billing.Application.Abstractions.LoggingInfo;

namespace Billing.WebApi.Extensions;

public static class HttpContextExtensions
{
    public static RequestMetadata GetRequestMetadata(this HttpContext httpContext)
    {
        string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString()
                         ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
                         ?? "unknown";

        string userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";

        return new RequestMetadata(ipAddress, userAgent);
    }
}