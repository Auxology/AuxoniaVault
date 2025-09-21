using Auth.SharedKernel;
using MediatR;

namespace Auth.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;