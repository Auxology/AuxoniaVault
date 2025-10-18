using Billing.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Billing.Application.Abstractions.Pipelines;

internal sealed class ExceptionPipelineBehavior<TRequest, TResponse>(
    ILogger<ExceptionPipelineBehavior<TRequest, TResponse>> logger,
    IDateTimeProvider dateTimeProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception exception)
        {
            var requestName = typeof(TRequest).Name;
            DateTimeOffset utcNow = dateTimeProvider.UtcNow;

            logger.LogError(exception,
                "Billing Service - Request {Name} failed at {DateTimeOffset} with exception: {ExceptionMessage}",
                requestName, utcNow, exception.Message);

            throw;
        }
    }
}