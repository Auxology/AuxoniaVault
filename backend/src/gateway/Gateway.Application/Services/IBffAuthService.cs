using Gateway.SharedKernel;
using Shared.Requests;

namespace Gateway.Application.Services;

public interface IBffAuthService
{
    Task<Result<string>> VerifyLoginHandlerAsync(VerifyLoginRequest request, CancellationToken cancellationToken = default);
}