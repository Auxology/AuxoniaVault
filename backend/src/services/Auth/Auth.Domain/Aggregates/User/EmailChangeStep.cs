namespace Auth.Domain.Aggregates.User;

public enum EmailChangeStep
{
    VerifyCurrent,
    VerifyNew,
    Completed
}