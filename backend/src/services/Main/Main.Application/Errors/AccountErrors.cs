using Main.SharedKernel;

namespace Main.Application.Errors;

internal static class AccountErrors
{
    public static Error NotFound => Error.NotFound
    (
        "Accounts.NotFound",
        "The specified account was not found."
    );
}