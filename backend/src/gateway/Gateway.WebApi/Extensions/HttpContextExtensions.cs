namespace Gateway.WebApi.Extensions;

public static class HttpContextExtensions
{
    private const int SessionExpiresInDays = 30;
    
    public static void SetAuthenticationCookie(this HttpContext httpContext, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(SessionExpiresInDays)
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
        => httpContext.Request.Cookies["refreshToken"];
}