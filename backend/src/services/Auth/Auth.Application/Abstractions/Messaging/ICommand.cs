using Auth.SharedKernel;
using MediatR;

namespace Auth.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;