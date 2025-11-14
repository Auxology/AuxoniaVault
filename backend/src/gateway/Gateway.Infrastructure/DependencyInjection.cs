using Gateway.Application.Abstractions.Authentication;
using Gateway.Infrastructure.Authentication;
using Gateway.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection
        AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddRedisCache(configuration)
            .AddCustomHttpClients(configuration)
            .AddAuthentication(configuration);
    
    private static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "GatewayInstance";
        });
        
        return services;
    }

    private static IServiceCollection AddCustomHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HttpClientSettings>(configuration.GetSection(HttpClientSettings.SectionName));
        
        var httpClientSettings = configuration.GetSection(HttpClientSettings.SectionName).Get<HttpClientSettings>()
            ?? throw new InvalidOperationException($"Configuration section '{HttpClientSettings.SectionName}' is not configured.");
        
        services.AddHttpClient("Auth", client =>
        {
            client.BaseAddress = new Uri(httpClientSettings.AuthServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        services.AddScoped<IAuthHttpClient, AuthHttpClient>();
        
        return services;
    }

    private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        
        services.AddScoped<ITokenStorage, TokenStorage>();
        
        return services;
    }
}