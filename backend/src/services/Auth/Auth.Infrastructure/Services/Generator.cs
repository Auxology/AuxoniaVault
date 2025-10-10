using System.Security.Cryptography;
using Auth.Application.Abstractions.Services;
using Auth.Domain.Constants;

namespace Auth.Infrastructure.Services;

internal sealed class Generator : IGenerator
{
    private const int RangeStart = 100000;
    private const int RangeEnd = 999999;
    private const int RecoveryCodeByteSize = 16;
    private const int RecoveryCodeLimit = UserConstants.MaxRecoveryCodes;

    public Task<int> GenerateVerificationCode()
    {
        int code = RandomNumberGenerator.GetInt32(RangeStart, RangeEnd);

        return Task.FromResult(code);
    }

    public Task<string> GenerateRecoveryCode()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(RecoveryCodeByteSize);
        
        string recoveryCode = Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return Task.FromResult(recoveryCode);
    }

    public async Task<string[]> GenerateRecoveryCodesAsync()
    {
        var tasks = Enumerable.Range(0, RecoveryCodeLimit)
            .Select(_ => GenerateRecoveryCode())
            .ToArray();

        return await Task.WhenAll(tasks);
    }
}