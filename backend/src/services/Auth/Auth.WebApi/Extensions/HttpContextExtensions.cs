using Auth.Application.Abstractions.LoggingInfo;
using Auth.Domain.Constants;

namespace Auth.WebApi.Extensions;

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

    public static void SetAuthenticationCookie(this HttpContext httpContext, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(SessionConstants.ExpiresInDays)
        };

        httpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    public static void RemoveAuthenticationCookie(this HttpContext httpContext)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        };

        httpContext.Response.Cookies.Delete("refreshToken", cookieOptions);
    }
    
    public static string? GetRefreshTokenFromCookie(this HttpContext httpContext)
    {
        return httpContext.Request.Cookies["refreshToken"];
    }
}