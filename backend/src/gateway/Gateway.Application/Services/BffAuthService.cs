using Gateway.Application.Abstractions.Authentication;
using Gateway.SharedKernel;
using Shared.Requests;

namespace Gateway.Application.Services;

internal sealed class BffAuthService(IAuthHttpClient authHttpClient, ITokenStorage tokenStorage) : IBffAuthService
{
    public async Task<Result<string>> VerifyLoginHandlerAsync(VerifyLoginRequest request, CancellationToken cancellationToken = default)
    {
        Result<VerifyLoginResponse> apiCallResult = await authHttpClient.CallVerifyLoginAsync(request, cancellationToken);

        if (apiCallResult.IsFailure)
            return Result.Failure<string>(apiCallResult.Error);
        
        string accessToken = apiCallResult.Value.AccessToken;
        string refreshToken = apiCallResult.Value.RefreshToken;

        await tokenStorage.StoreAccessTokenAsync
        (
            refreshToken: refreshToken,
            accessToken: accessToken,
            cancellationToken
        );
        
        return Result.Success(accessToken);
    }
}