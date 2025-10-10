using Auth.Application.Abstractions.Authentication;
using Isopoh.Cryptography.Argon2;

namespace Auth.Infrastructure.Authentication;

internal sealed class SecretHasher : ISecretHasher
{
    public string Hash(string secret) => Argon2.Hash(secret);
    
    public async Task<string> HashAsync(string secret, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => Argon2.Hash(secret), cancellationToken);
    }

    public bool Verify(string hash, string secret) => Argon2.Verify(hash, secret);
    
    public Task<bool> VerifyAsync(string hash, string secret, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Argon2.Verify(hash, secret), cancellationToken);
    }
}