using Amazon.SimpleEmail;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure.Emails;

namespace Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection
        AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddAmazonSes(configuration)
            .AddMassTransit(configuration);

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