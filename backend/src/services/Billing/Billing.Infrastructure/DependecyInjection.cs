using System.Text;
using Billing.Application.Abstractions.Authentication;
using Billing.Application.Abstractions.Database;
using Billing.Application.Abstractions.Messaging;
using Billing.Application.Abstractions.Services;
using Billing.Domain.Events;
using Billing.Infrastructure.Authentication;
using Billing.Infrastructure.Database;
using Billing.Infrastructure.DomainEvents;
using Billing.Infrastructure.IntegrationEvents.SubscriptionActivated;
using Billing.Infrastructure.Services;
using Billing.Infrastructure.Settings;
using Billing.Infrastructure.Time;
using Billing.SharedKernel;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Stripe;

namespace Billing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection
        AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddStripe(configuration)
            .AddMassTransit(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal()
            .AddConsumers();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IDomainEventDispatcher, DomainEventDispatcher>();
        
        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (connectionString is null)
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<BillingDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IBillingDbContext>(provider => provider.GetRequiredService<BillingDbContext>());

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

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddStripe(this IServiceCollection services, IConfiguration configuration)
    {
        var stripeApiKey = configuration["Stripe:ApiKey"];

        if (string.IsNullOrEmpty(stripeApiKey))
            throw new InvalidOperationException("Stripe API key is not configured.");

        StripeConfiguration.ApiKey = stripeApiKey;

        services.AddSingleton<IStripeClient>(new StripeClient(stripeApiKey));
        services.Configure<StripeSettings>(configuration.GetSection(StripeSettings.SectionName));
        services.AddTransient<IStripeCheckoutService, StripeCheckoutService>();
        services.AddTransient<IStripeWebhookService, StripeWebhookService>();
        
        // Register Stripe services for dependency injection
        services.AddTransient<SubscriptionService>();
        services.AddTransient<ProductService>();
        services.AddTransient<PriceService>();

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
        services.AddTransient<INotificationHandler<DomainEventNotification<SubscriptionActivatedDomainEvent>>,
            SubscriptionActivatedDomainEventHandler>();
        
        return services;
    }
}