namespace Billing.Application.Abstractions.LoggingInfo;

public record RequestMetadata
(
    string IpAddress,
    string UserAgent
);