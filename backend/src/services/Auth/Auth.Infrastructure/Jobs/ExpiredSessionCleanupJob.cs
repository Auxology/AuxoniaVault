using Auth.Application.Abstractions.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Auth.Infrastructure.Jobs;

internal sealed class ExpiredSessionCleanupJob(IAuthDbContext dbContext, ILogger<ExpiredSessionCleanupJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;

        try
        {
            var batchedSize = 1000;
            int totalDeleted = 0;

            while (true)
            {
                var expiredSessionIds = dbContext.Sessions
                    .Where(s => s.ExpiresAt <= utcNow)
                    .Take(batchedSize)
                    .Select(s => s.Id)
                    .ToList();

                if (!expiredSessionIds.Any())
                    break;
                
                await dbContext.Sessions
                    .Where(s => expiredSessionIds.Contains(s.Id))
                    .ExecuteDeleteAsync();
                
                totalDeleted += expiredSessionIds.Count;
                logger.LogInformation("Deleted {Count} expired sessions in this batch.", expiredSessionIds.Count);
                
                await Task.Delay(100);
            }
            
            logger.LogInformation("Expired session cleanup job completed. Total deleted sessions: {TotalDeleted}", totalDeleted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing the expired session cleanup job.");
        }
    }
}