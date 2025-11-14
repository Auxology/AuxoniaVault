using Shared.Requests;

namespace Gateway.Application.Abstractions.Authentication;

public interface IAuthHttpClient
{
    Task<HttpResponseMessage> CallVerifyLoginAsync(VerifyLoginRequest request, CancellationToken cancellationToken = default);
}