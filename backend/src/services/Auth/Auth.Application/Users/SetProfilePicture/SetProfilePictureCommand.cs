using Auth.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Http;

namespace Auth.Application.Users.SetProfilePicture;

public record SetProfilePictureCommand
(
    IFormFile File
) : ICommand;