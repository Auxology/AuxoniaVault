using Main.Domain.Constants;
using Main.Domain.Errors;
using Main.Domain.ValueObjects;
using Main.SharedKernel;

namespace Main.Domain.Aggregates.Account;

public sealed class Account : Entity, IAggregateRoot
{
    public UserId Id { get; private set; }
    
    public string AccountName { get; private set; }
    
    public string AccountEmail { get; private set; }
    
    public int AccountTier { get; private set; }
    
    public long MaxStorageInBytes { get; private set; }
    
    public long UsedStorageInBytes { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    private Account() {} // For EF Core

    private Account
    (
        UserId id,
        string accountName,
        string accountEmail,
        DateTimeOffset utcNow
    )
    {
        Id = id;
        AccountName = accountName;
        AccountEmail = accountEmail;
        AccountTier = AccountConstants.FreeTier;
        MaxStorageInBytes = AccountConstants.MaxStorageInBytesFreeTier;
        UsedStorageInBytes = 0;
        CreatedAt = utcNow;
    }

    public static Result<Account> Create(UserId id, string accountName, string accountEmail,
        IDateTimeProvider dateTimeProvider)
    {
        if (id.IsEmpty())
            return Result.Failure<Account>(AccountErrors.UserIdRequired);
        
        if (string.IsNullOrWhiteSpace(accountName))
            return Result.Failure<Account>(AccountErrors.AccountNameRequired);
        
        if (string.IsNullOrWhiteSpace(accountEmail))
            return Result.Failure<Account>(AccountErrors.AccountEmailRequired);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        Account account = new Account
        (
            id: id,
            accountName: accountName,
            accountEmail: accountEmail,
            utcNow: utcNow
        );
        
        return Result.Success(account);
    }

    public Result ChangeTier(int newTier, IDateTimeProvider dateTimeProvider)
    {
        if (newTier < 0)
            return Result.Failure(AccountErrors.InvalidAccountTier);
        
        if (AccountTier == newTier)
            return Result.Success();
        
        AccountTier = newTier;

        MaxStorageInBytes = newTier switch
        {
            AccountConstants.FreeTier => AccountConstants.MaxStorageInBytesFreeTier,
            AccountConstants.ProTier => AccountConstants.MaxStorageInBytesProTier,
            _ => MaxStorageInBytes
        };
        
        UpdatedAt = dateTimeProvider.UtcNow;
        
        return Result.Success();
    }

    public Result<bool> CanUploadFile(long fileSize)
    {
        if (fileSize <= 0)
            return Result.Failure<bool>(AccountErrors.InvalidFileSize);
        
        long projectedUsage = UsedStorageInBytes + fileSize;
        
        if (projectedUsage > MaxStorageInBytes)
            return Result.Failure<bool>(AccountErrors.ExceedsStorageLimit);
        
        return Result.Success(true);
    }
}