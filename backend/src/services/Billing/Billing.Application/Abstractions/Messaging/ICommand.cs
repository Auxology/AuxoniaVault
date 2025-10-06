using Billing.SharedKernel;
using MediatR;

namespace Billing.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;