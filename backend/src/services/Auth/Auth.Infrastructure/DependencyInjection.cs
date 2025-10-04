using System.Text;
using Amazon.S3;
using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Abstractions.Storage;
using Auth.Domain.Events;
using Auth.Infrastructure.Authentication;
using Auth.Infrastructure.Database;
using Auth.Infrastructure.DomainEvents;
using Auth.Infrastructure.IntegrationEvents.EmailChanged;
using Auth.Infrastructure.IntegrationEvents.EmailChangeRequested;
using Auth.Infrastructure.IntegrationEvents.LoginRequested;
using Auth.Infrastructure.Jobs;
using Auth.Infrastructure.Storage;
using Auth.Infrastructure.Time;
using Auth.SharedKernel;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quartz;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection
        AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal()
            .AddStorage(configuration)
            .AddMassTransit(configuration)
            .AddConsumers();

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
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<ISecretHasher, SecretHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqHost = configuration["RabbitMQ:Host"];
        var rabbitMqPort = configuration["RabbitMQ:Port"];
        var rabbitMqUsername = configuration["RabbitMQ:Username"];
        var rabbitMqPassword = configuration["RabbitMQ:Password"];

        if (string.IsNullOrEmpty(rabbitMqUsername) || string.IsNullOrEmpty(rabbitMqPassword) ||
            string.IsNullOrEmpty(rabbitMqHost) || string.IsNullOrEmpty(rabbitMqPort))
            throw new InvalidOperationException("RabbitMQ credentials are not configured properly.");

        services.AddMassTransit(x =>
        {
            x.AddConsumers(typeof(DependencyInjection).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqHost, ushort.Parse(rabbitMqPort), "/", h =>
                {
                    h.Username(rabbitMqUsername);
                    h.Password(rabbitMqPassword);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static IServiceCollection AddConsumers(this IServiceCollection services)
    {
        services.AddTransient<INotificationHandler<DomainEventNotification<LoginRequestedDomainEvent>>,
            LoginRequestedDomainEventHandler>();

        services.AddTransient<INotificationHandler<DomainEventNotification<EmailChangeRequestedDomainEvent>>,
            EmailChangeRequestedDomainEventHandler>();

        services.AddTransient<INotificationHandler<DomainEventNotification<EmailChangeCurrentEmailVerifiedDomainEvent>>,
            EmailChangeCurrentEmailVerifiedDomainEventHandler>();

        services.AddTransient<INotificationHandler<DomainEventNotification<EmailChangedDomainEvent>>,
            EmailChangedDomainEventHandler>();

        return services;
    }

    private static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<S3Settings>(configuration.GetSection("S3Settings"));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var s3Settings = sp.GetRequiredService<IOptions<S3Settings>>().Value;

            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(s3Settings.RegionName)
            };

            return new AmazonS3Client(config);
        });

        services.AddScoped<IStorageServices, StorageServices>();

        return services;
    }
}