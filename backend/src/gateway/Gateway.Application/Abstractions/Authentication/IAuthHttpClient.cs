using Gateway.SharedKernel;
using Shared.Requests;
using Shared.Responses;

namespace Gateway.Application.Abstractions.Authentication;

public interface IAuthHttpClient
{
    Task<Result<VerifyLoginResponse>> CallVerifyLoginAsync(VerifyLoginRequest request, CancellationToken cancellationToken = default);
    
    Task<Result<LoginWithRefreshTokenResponse>> CallLoginWithRefreshTokenAsync(LoginWithRefreshTokenRequest request, CancellationToken cancellationToken = default);
}