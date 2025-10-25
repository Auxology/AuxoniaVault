using System.Text.Json;
using Auth.Domain.Aggregates.AuditLog;
using Auth.Domain.Events;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;

namespace Auth.Infrastructure.AuditLogs;

internal static class AuditLogFactory
{
    public static AuditLog CreateFromDomainEvent(IDomainEvent domainEvent, IDateTimeProvider dateTimeProvider)
    {
        return domainEvent switch
        {
            UserCreatedDomainEvent @event => CreateAuditLog
            (
                userId: UserId.UnsafeFromGuid(@event.UserId),
                action: AuditAction.UserCreated,
                entityType: "User",
                entityId: @event.UserId.ToString(),
                ipAddress: @event.IpAddress,
                userAgent: @event.UserAgent,
                dateTimeProvider: dateTimeProvider,
                details: $"Email: {@event.Email}, Name: {@event.Name}"
            ),
            EmailChangeRequestedDomainEvent @event => CreateAuditLog
            (
                userId: UserId.UnsafeFromGuid(@event.UserId),
                action: AuditAction.EmailChangeRequested,
                entityType: "User",
                entityId: @event.UserId.ToString(),
                ipAddress: @event.IpAddress,
                userAgent: @event.UserAgent,
                dateTimeProvider: dateTimeProvider,
                details: $"CurrentEmail: {@event.CurrentEmail} OTP: {"***"}"
            ),
            EmailChangeCurrentEmailVerifiedDomainEvent @event => CreateAuditLog
            (
                userId: UserId.UnsafeFromGuid(@event.UserId),
                action: AuditAction.EmailChangeCurrentEmailVerified,
                entityType: "User",
                entityId: @event.UserId.ToString(),
                ipAddress: @event.IpAddress,
                userAgent: @event.UserAgent,
                dateTimeProvider: dateTimeProvider
            ),
            EmailChangedDomainEvent @event => CreateAuditLog
            (
                userId: UserId.UnsafeFromGuid(@event.UserId),
                action: AuditAction.EmailChanged,
                entityType: "User",
                entityId: @event.UserId.ToString(),
                ipAddress: @event.IpAddress,
                userAgent: @event.UserAgent,
                dateTimeProvider: dateTimeProvider,
                details: $"NewEmail: {@event.NewEmail}"
            ),
            _ => throw new NotSupportedException($"Domain event of type {domainEvent.GetType().Name} is not supported for audit logging.")
        };
    }
    
    private static AuditLog CreateAuditLog
    (
        UserId userId,
        AuditAction action,
        string entityType,
        string entityId,
        string ipAddress,
        string userAgent,
        IDateTimeProvider dateTimeProvider,
        string? details = null
    )
    {
        string? detailsJson = details is not null 
            ? JsonSerializer.Serialize(details) 
            : null;

        Result<AuditLog> auditLogResult = AuditLog.Create
        (
            userId,
            action,
            entityType,
            entityId,
            dateTimeProvider,
            detailsJson,
            ipAddress,
            userAgent
        );

        if (auditLogResult.IsFailure)
            throw new InvalidOperationException($"Failed to create AuditLog: {auditLogResult.Error}");
        
        return auditLogResult.Value;
    }
}