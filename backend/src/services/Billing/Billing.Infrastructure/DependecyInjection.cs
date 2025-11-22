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
using Billing.Infrastructure.IntegrationEvents.SubscriptionCanceled;
using Billing.Infrastructure.Services;
using Billing.Infrastructure.Settings;
using Billing.Infrastructure.Time;
using Billing.Infrastructure.Webhooks;
using Billing.Infrastructure.Webhooks.Services;
using Billing.SharedKernel;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Abstractions.Authentication;
using Stripe;

namespace Billing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection
        AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddOtel()
            .AddServices()
            .AddDatabase(configuration)
            .AddStripe(configuration)
            .AddMassTransit(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal()
            .AddConsumers()
            .AddRedisCache(configuration);

    private static IServiceCollection AddOtel(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("BillingService"))
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
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
                
                o.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var blackListCache = context.HttpContext.RequestServices
                            .GetRequiredService<ISessionBlacklistCache>();

                        Guid sessionId = context.Principal.GetSessionId();

                        bool isBlacklisted = await blackListCache.IsSessionBlacklistedAsync(sessionId);

                        if (isBlacklisted)
                            context.Fail("This session has been revoked.");
                    }
                };
            });

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ISessionBlacklistCache, SessionBlacklistCache>();
        
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
        
        services.AddSingleton<SubscriptionService>();
        services.AddSingleton<Stripe.Checkout.SessionService>();
        services.AddSingleton<Stripe.BillingPortal.SessionService>();
        services.AddSingleton<CustomerService>();
        services.AddSingleton<ProductService>();
        services.AddSingleton<PriceService>();
        
        services.AddTransient<IStripeCheckoutService, StripeCheckoutService>();
        services.AddTransient<IStripeSubscriptionFetcher, StripeSubscriptionFetcher>();
        services.AddTransient<IStripeWebhookMapper, StripeWebhookMapper>();
        services.AddTransient<IStripeBillingPortalService, StripeBillingPortalService>();
        services.AddSingleton<IStripePriceTierMapper, StripePriceTierMapper>();
        services.AddScoped<IStripeWebhookHandler, StripeWebhookHandler>();

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
        
        services.AddTransient<INotificationHandler<DomainEventNotification<SubscriptionCanceledDomainEvent>>,
            SubscriptionCanceledDomainEventHandler>();
        
        return services;
    }
    
    private static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "AuxoniaVault";
        });
        
        return services;
    }
}