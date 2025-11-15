using Gateway.Application.Services;
using Gateway.WebApi.Infrastructure;
using Gateway.WebApi.Transforms;
using Yarp.ReverseProxy.Transforms;

namespace Gateway.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddTransforms(builderContext =>
            {
                builderContext.AddRequestTransform(async transformContext =>
                {
                    IBffAuthService bffAuthService = transformContext.HttpContext.RequestServices
                        .GetRequiredService<IBffAuthService>();
                        
                    AuthorizedRequestTransform transform = new(bffAuthService);
                    await transform.ApplyAsync(transformContext);
                });
            });
        
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        return services;
    }
}