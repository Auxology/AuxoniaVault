using Auth.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Abstractions.Pipelines;

internal sealed class LoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger,
    IDateTimeProvider dateTimeProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        logger.LogInformation("Auth Service - Processing request {Request} at {Time}", requestName, dateTimeProvider.UtcNow);

        DateTimeOffset startTime = dateTimeProvider.UtcNow;

        TResponse response = await next(cancellationToken);

        DateTimeOffset endTime = dateTimeProvider.UtcNow;
        TimeSpan duration = endTime - startTime;

        if (response is Result { IsSuccess: true })
        {
            logger.LogInformation("Auth Service - Completed request {Request} at {Time} (Duration: {Duration} ms)",
                requestName, dateTimeProvider.UtcNow, duration.TotalMilliseconds);
        }
        else if (response is Result { IsFailure: true } result)
        {
            logger.LogError("Auth Service - Completed request {Request} at {Time} with error (Duration: {Duration} ms). Error: {Error}",
                requestName, dateTimeProvider.UtcNow, duration.TotalMilliseconds, result.Error);
        }
        else
        {
            logger.LogInformation("Auth Service - Completed request {Request} at {Time} (Duration: {Duration} ms)",
                requestName, dateTimeProvider.UtcNow, duration.TotalMilliseconds);
        }

        return response;
    }
}