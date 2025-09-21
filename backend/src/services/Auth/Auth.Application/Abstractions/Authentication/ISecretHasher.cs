namespace Auth.Application.Abstractions.Authentication;

public interface ISecretHasher
{
    string Hash(string secret);
    
    bool Verify(string hash, string secret);
}