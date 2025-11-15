using System.Net.Http.Headers;
using Gateway.Application.Services;
using Gateway.WebApi.Extensions;
using Yarp.ReverseProxy.Transforms;

namespace Gateway.WebApi.Transforms;

internal sealed class AuthorizedRequestTransform(IBffAuthService bffAuthService) : RequestTransform
{
    public override async ValueTask ApplyAsync(RequestTransformContext context)
    {
        HttpContext httpContext = context.HttpContext;

        string? refreshToken = httpContext.GetRefreshTokenFromCookie();

        if (string.IsNullOrWhiteSpace(refreshToken))
            return;
        
        var accessTokenResult = await bffAuthService.GetOrRefreshAccessTokenAsync(refreshToken);

        if (accessTokenResult.IsFailure)
            return;

        if (refreshToken != accessTokenResult.Value.RefreshToken)
        {
            httpContext.RemoveAuthenticationCookie();
            httpContext.SetAuthenticationCookie(accessTokenResult.Value.RefreshToken);
        }
        
        string accessToken = accessTokenResult.Value.AccessToken;
        
        context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}