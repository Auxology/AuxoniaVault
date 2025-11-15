using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Main.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out Guid parsedUserId) ?
            parsedUserId :
            throw new ApplicationException("User id is unavailable");
    }
    
    public static Guid GetSessionId(this ClaimsPrincipal? principal)
    {
        string? sessionId = principal?.FindFirstValue(JwtRegisteredClaimNames.Sid);

        return Guid.TryParse(sessionId, out Guid parsedSessionId) ?
            parsedSessionId :
            throw new ApplicationException("Session id is unavailable");
    }
}
