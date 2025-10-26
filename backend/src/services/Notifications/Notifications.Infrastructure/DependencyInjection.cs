using Amazon.SimpleEmail;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure.Emails;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection
        AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddOtel()
            .AddAmazonSes(configuration)
            .AddMassTransit(configuration);

    private static IServiceCollection AddOtel(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("NotificationsService"))
            .WithMetrics(metrics => 
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddNpgsqlInstrumentation())
            .WithTracing(tracing =>
                tracing
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddNpgsql()
                    .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
            )
            .UseOtlpExporter();
        
        services.AddLogging(builder => builder.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.ParseStateValues = true;
        }));
        
        return services;
    }
    
    private static IServiceCollection AddAmazonSes(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDefaultAWSOptions(configuration.GetAWSOptions());

        services.AddAWSService<IAmazonSimpleEmailService>();

        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.ConfigurationSectionName));
        services.AddScoped<EmailTemplateService>();

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
}