namespace Shared.Contracts;

public sealed record CustomerTierInitializedContract
(
    Guid UserId,
    int Tier,
    DateTimeOffset CreatedAt
) ;