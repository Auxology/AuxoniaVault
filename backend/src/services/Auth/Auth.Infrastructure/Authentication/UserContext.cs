using Auth.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Auth.Infrastructure.Authentication;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;

            var userIdHeader = context?.Request.Headers["X-User-Id"].FirstOrDefault();

            if (!string.IsNullOrEmpty(userIdHeader) && Guid.TryParse(userIdHeader, out var headerUserId))
            {
                return headerUserId;
            }

            var userIdClaim = context?.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var claimUserId))
            {
                return claimUserId;
            }

            return Guid.Empty;
        }
    }
}