using Auth.Application.Abstractions.Authentication;
using Isopoh.Cryptography.Argon2;

namespace Auth.Infrastructure.Authentication;

internal sealed class SecretHasher : ISecretHasher
{
    public string Hash(string secret) => Argon2.Hash(secret);

    public bool Verify(string hash, string secret) => Argon2.Verify(hash, secret);
}