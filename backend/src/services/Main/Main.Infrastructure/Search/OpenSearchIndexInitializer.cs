using Main.Infrastructure.Search.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Main.Infrastructure.Search;

internal sealed class OpenSearchIndexInitializer(IServiceProvider serviceProvider, ILogger<OpenSearchIndexInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Initializing OpenSearch index...");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var searchService = scope.ServiceProvider.GetRequiredService<IOpenSearchService>();

            var result = await searchService.InitializeIndexAsync(cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("OpenSearch index initialized successfully");
            }
            else
            {
                logger.LogWarning("OpenSearch index initialization failed: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing OpenSearch index");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}