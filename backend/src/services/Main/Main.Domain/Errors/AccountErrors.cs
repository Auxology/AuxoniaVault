using Main.SharedKernel;

namespace Main.Domain.Errors;

internal static class AccountErrors
{
    public static Error UserIdRequired => Error.Validation
    (
        "Accounts.UserIdRequired",
        "The user id is required."
    );

    public static Error AccountNameRequired => Error.Validation
    (
        "Accounts.AccountNameRequired",
        "The account name is required."
    );

    public static Error AccountEmailRequired => Error.Validation
    (
        "Accounts.AccountEmailRequired",
        "The account email is required."
    );
    
    public static Error InvalidAccountTier => Error.Validation
    (
        "Accounts.InvalidAccountTier",
        "The account tier is invalid."
    );
    
    public static Error InvalidFileSize => Error.Validation
    (
        "Accounts.InvalidFileSize",
        "The file size is invalid."
    );
    
    public static Error ExceedsStorageLimit => Error.Conflict
    (
        "Accounts.ExceedsStorageLimit",
        "The file upload would exceed the account's storage limit."
    );
}