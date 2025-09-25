using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Infrastructure.Authentication;
using Auth.Infrastructure.Database;
using Auth.Infrastructure.DomainEvents;
using Auth.Infrastructure.Jobs;
using Auth.Infrastructure.Time;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();
    
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        services.AddTransient<IDomainEventDispatcher, DomainEventDispatcher>();
        
        services.AddQuartz(configure =>
        {
            var jobKey = new JobKey("ExpiredSessionCleanup");
            configure
                .AddJob<ExpiredSessionCleanupJob>(jobKey)
                .AddTrigger(trigger =>
                    trigger.ForJob(jobKey)
                        .WithIdentity("ExpiredSessionCleanup-trigger")
                        // Run daily at 2 AM
                        .WithCronSchedule("0 0 2 * * ?")
                        .WithDescription("Daily cleanup of expired sessions"));

            var loginVerificationJobKey = new JobKey("ExpiredLoginVerificationCleanup");
            configure
                .AddJob<ExpiredLoginVerificationCleanupJob>(loginVerificationJobKey)
                .AddTrigger(trigger =>
                    trigger.ForJob(loginVerificationJobKey)
                        .WithIdentity("ExpiredLoginVerificationCleanup-trigger")
                        // Run every 30 minutes (login codes expire in 10 minutes)
                        .WithCronSchedule("0 */30 * * * ?")
                        .WithDescription("Cleanup expired login verification codes"));
        });
        
        return services;
    }
    
    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (connectionString is null)
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            options.UseSnakeCaseNamingConvention();
        });
        
        services.AddScoped<IAuthDbContext>(provider => provider.GetRequiredService<AuthDbContext>());
        
        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication();

        services.AddHttpContextAccessor();
        
        services.AddSingleton<ISecretHasher, SecretHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        
        return services;
    }
    
    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();
        
        return services;
    }
}