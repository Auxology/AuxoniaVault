using Auth.Domain.Aggregates.User;

namespace Auth.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string Create(User user);
}