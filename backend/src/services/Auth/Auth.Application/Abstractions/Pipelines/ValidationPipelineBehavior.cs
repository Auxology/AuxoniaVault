using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Auth.Application.Abstractions.Pipelines;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse>
    (IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);
        
        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] results =
            await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        ValidationFailure[] failures = results
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToArray();
        
        if (failures.Length > 0)
            throw new ValidationException(failures);
        
        return await next(cancellationToken);
    }
}
