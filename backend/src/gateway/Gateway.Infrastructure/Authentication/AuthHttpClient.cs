using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Gateway.Application.Abstractions.Authentication;
using Gateway.Infrastructure.Abstractions;
using Gateway.Infrastructure.Errors;
using Gateway.Infrastructure.Settings;
using Gateway.SharedKernel;
using Microsoft.Extensions.Options;
using Shared.Requests;

namespace Gateway.Infrastructure.Authentication;

internal sealed class AuthHttpClient(IHttpClientFactory httpClientFactory, IOptions<HttpClientSettings> httpSettings) : IAuthHttpClient
{
    private readonly string _endpoint = httpSettings.Value.AuthServiceVerifyLoginEndpoint;
    
    public async Task<Result<VerifyLoginResponse>> CallVerifyLoginAsync(VerifyLoginRequest request, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Auth");
        
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(_endpoint, request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            VerifyLoginResponse? verifyLoginResponse =
                await response.Content.ReadFromJsonAsync<VerifyLoginResponse>(cancellationToken: cancellationToken);

            if (verifyLoginResponse is null)
                return Result.Failure<VerifyLoginResponse>(AuthErrors.FailedToDeserialize);
            
            return Result.Success(verifyLoginResponse);
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        ApiErrorResponseDto? errorResponse = JsonSerializer.Deserialize<ApiErrorResponseDto>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (errorResponse is null)
            return Result.Failure<VerifyLoginResponse>(AuthErrors.FailedToDeserialize);

        return Result.Failure<VerifyLoginResponse>(errorResponse.ToError());
    }
}