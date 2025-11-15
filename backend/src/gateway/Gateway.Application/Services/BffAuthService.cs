using Gateway.Application.Abstractions.Authentication;
using Gateway.Application.DTOs;
using Gateway.SharedKernel;
using Shared.Requests;
using Shared.Responses;

namespace Gateway.Application.Services;

internal sealed class BffAuthService(IAuthHttpClient authHttpClient, ITokenStorage tokenStorage, ITokenValidator tokenValidator) : IBffAuthService
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
        
        return Result.Success(refreshToken);
    }

    public async Task<Result<TokenPair>> GetOrRefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        Result<string> tokenResult = await tokenStorage.GetAccessTokenAsync(refreshToken, cancellationToken);

        if (tokenResult.IsFailure || !await tokenValidator.IsTokenValidAsync(tokenResult.Value, cancellationToken))
        {
            LoginWithRefreshTokenRequest request = new LoginWithRefreshTokenRequest(RefreshToken: refreshToken);
            
            Result<LoginWithRefreshTokenResponse> apiCallResult = await authHttpClient.CallLoginWithRefreshTokenAsync(request, cancellationToken);
            
            if (apiCallResult.IsFailure)
                return Result.Failure<TokenPair>(apiCallResult.Error);
            
            string newAccessToken = apiCallResult.Value.AccessToken;
            string newRefreshToken = apiCallResult.Value.RefreshToken;
            
            await tokenStorage.StoreAccessTokenAsync
            (
                refreshToken: newRefreshToken,
                accessToken: newAccessToken,
                cancellationToken
            );
            
            TokenPair newTokenPair = new TokenPair
            (
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken
            );
            
            return Result.Success(newTokenPair);
        }
        
        TokenPair existingTokenPair = new TokenPair
        (
            AccessToken: tokenResult.Value,
            RefreshToken: refreshToken
        );
        
        return Result.Success(existingTokenPair);
    }
}