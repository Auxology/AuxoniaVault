namespace Auth.Domain.Aggregates.Session;

public enum SessionStatus
{
    Active,
    Revoked,
    Expired
}