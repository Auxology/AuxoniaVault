using Gateway.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) =>
        services
            .AddAuthServices();
    
    private static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddScoped<IBffAuthService, BffAuthService>();
        
        return services;
    }
}