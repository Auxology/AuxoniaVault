using Gateway.WebApi.Infrastructure;

namespace Gateway.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy().LoadFromConfig(configuration.GetSection("ReverseProxy"));
        
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        return services;
    }
}