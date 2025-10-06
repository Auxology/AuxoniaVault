using Billing.SharedKernel;
using MediatR;

namespace Billing.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;