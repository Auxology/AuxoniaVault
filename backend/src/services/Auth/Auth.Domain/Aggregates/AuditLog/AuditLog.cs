using Auth.Domain.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;

namespace Auth.Domain.Aggregates.AuditLog;

public class AuditLog : Entity, IAggregateRoot
{
    public AuditLogId Id { get; private set; }
    
    public UserId UserId { get; private set; }
    
    public AuditAction Action { get; private set; }
    
    public string EntityType { get; private set; }
    
    public string EntityId { get; private set; }
    
    public string? Details { get; private set; }
    
    public string? IpAddress { get; private set; }
    
    public string? UserAgent { get; private set; }
    
    public DateTimeOffset OccurredAt { get; private set; }
    
    private AuditLog() {} // For EF CORE

    private AuditLog
    (
        UserId userId,
        AuditAction action,
        string entityType,
        string entityId,
        string? details,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset utcNow
    )

    {
        Id = AuditLogId.New();
        UserId = userId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Details = details;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        OccurredAt = utcNow;
    }
    
    public static Result<AuditLog> Create
    (
        UserId userId,
        AuditAction action,
        string entityType,
        string entityId,
        IDateTimeProvider dateTimeProvider,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null
    )
    {
        if (userId.IsEmpty())
            return Result.Failure<AuditLog>(AuditLogErrors.UserIdRequired);
        
        if (string.IsNullOrWhiteSpace(entityType))
            return Result.Failure<AuditLog>(AuditLogErrors.EntityTypeRequired);
        
        if (string.IsNullOrWhiteSpace(entityId))
            return Result.Failure<AuditLog>(AuditLogErrors.EntityIdRequired);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        var auditLog = new AuditLog
        (
            userId,
            action,
            entityType,
            entityId,
            details,
            ipAddress,
            userAgent,
            utcNow
        );
        
        return Result.Success(auditLog);
    }
}