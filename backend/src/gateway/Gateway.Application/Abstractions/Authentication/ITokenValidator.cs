namespace Gateway.Application.Abstractions.Authentication;

public interface ITokenValidator
{
    Task<bool> IsTokenValidAsync(string token, CancellationToken cancellationToken = default);
}