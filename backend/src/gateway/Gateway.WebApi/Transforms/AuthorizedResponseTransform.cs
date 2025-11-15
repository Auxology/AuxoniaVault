using Gateway.WebApi.Extensions;
using Yarp.ReverseProxy.Transforms;

namespace Gateway.WebApi.Transforms;

internal sealed class AuthorizedResponseTransform : ResponseTransform
{
    public override async ValueTask ApplyAsync(ResponseTransformContext context)
    {
        HttpContext httpContext = context.HttpContext;
        
        if (context.ProxyResponse?.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            httpContext.RemoveAuthenticationCookie();
    }
}