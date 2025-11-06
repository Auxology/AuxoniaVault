using Main.SharedKernel;
using MediatR;

namespace Main.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;