namespace Auth.Application.Abstractions.LoggingInfo;

public record RequestMetadata
(
    string IpAddress,
    string UserAgent
);