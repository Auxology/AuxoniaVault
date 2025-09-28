using Auth.Application.Abstractions.Messaging;

namespace Auth.Application.Users.ChangeName;

public record ChangeNameCommand
(
    string Name
) : ICommand<string>;