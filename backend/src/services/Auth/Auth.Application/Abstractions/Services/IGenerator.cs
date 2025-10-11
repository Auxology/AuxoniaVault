namespace Auth.Application.Abstractions.Services;

public interface IGenerator
{
    Task<int> GenerateVerificationCode();
    
    Task<string> GenerateRecoveryCode();

    Task<string[]> GenerateRecoveryCodesAsync();
    
    Task<string> GenerateUniqueIdentifier();
};