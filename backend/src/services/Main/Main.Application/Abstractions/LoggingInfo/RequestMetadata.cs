namespace Main.Application.Abstractions.LoggingInfo;

public record RequestMetadata
(
    string IpAddress,
    string UserAgent
);