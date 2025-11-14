using Gateway.SharedKernel;
using Shared.Requests;

namespace Gateway.Application.Abstractions.Authentication;

public interface IAuthHttpClient
{
    Task<Result<VerifyLoginResponse>> CallVerifyLoginAsync(VerifyLoginRequest request, CancellationToken cancellationToken = default);
}