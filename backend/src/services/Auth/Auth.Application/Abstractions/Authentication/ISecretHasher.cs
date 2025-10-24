namespace Auth.Application.Abstractions.Authentication;

public interface ISecretHasher
{
    string Hash(string secret);
    
    Task<string> HashAsync(string secret, CancellationToken cancellationToken = default);
        
    bool Verify(string hash, string secret);
    
    Task<bool> VerifyAsync(string hash, string secret, CancellationToken cancellationToken = default);
    
    Task<bool> OneToManyVerifyAsync(IEnumerable<string> hashes, string secret, CancellationToken cancellationToken = default);
}