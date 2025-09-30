namespace Shared.Contracts;

public sealed record EmailChangeCurrentEmailVerifiedContract
(
    string newEmail,
    int newOtp,
    DateTimeOffset RequestedAt
);