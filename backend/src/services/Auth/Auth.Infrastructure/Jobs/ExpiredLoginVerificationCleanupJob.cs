using Auth.Application.Abstractions.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Auth.Infrastructure.Jobs;

internal sealed class ExpiredLoginVerificationCleanupJob(IAuthDbContext dbContext, ILogger<ExpiredLoginVerificationCleanupJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;

        try
        {
            var deletedCount = await dbContext.LoginVerifications
                .Where(lv => lv.ExpiresAt <= utcNow)
                .ExecuteDeleteAsync();
            
            logger.LogInformation("Expired login verification cleanup job completed. Deleted {DeletedCount} expired login verifications.", deletedCount);
        }
        
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing the expired login verification cleanup job.");
        }
    }
}