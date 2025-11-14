using Gateway.SharedKernel;

namespace Gateway.Infrastructure.Abstractions;

internal static class ApiErrorResponseDtoExtensions
{
    public static Error ToError(this ApiErrorResponseDto apiErrorResponseDto)
    {
        string code = apiErrorResponseDto.Title ?? "UnknownError";
        string description = apiErrorResponseDto.Detail ?? "An error occurred in the authentication service.";
        
        return (apiErrorResponseDto.Status ?? 500) switch
        {
            400 => Error.Validation(code, description),
            401 => Error.Unauthorized(code, description),
            404 => Error.NotFound(code, description),
            409 => Error.Conflict(code, description),
            _ => Error.Failure(code, description)
        };
    }
}