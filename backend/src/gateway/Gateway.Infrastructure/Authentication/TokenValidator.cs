using System.Text;
using Gateway.Application.Abstractions.Authentication;
using Gateway.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.Infrastructure.Authentication;

internal sealed class TokenValidator : ITokenValidator
{
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly JsonWebTokenHandler _tokenHandler;

    public TokenValidator(IOptions<JwtSettings> jwtSettings)
    {
        var settings = jwtSettings.Value;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));

        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = securityKey,
            ClockSkew = TimeSpan.Zero
        };

        _tokenHandler = new JsonWebTokenHandler();
    }
    
    public async Task<bool> IsTokenValidAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var result = await _tokenHandler.ValidateTokenAsync(token, _tokenValidationParameters);
            return result.IsValid;
        }
        catch
        {
            return false;
        }
    }
}