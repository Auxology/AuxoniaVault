using Auth.Domain.Aggregates.AuditLog;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.AuditLogs;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.HasKey(al => al.Id);
        
        b.Property(al => al.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => AuditLogId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();
        
        b.Property(al => al.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();

        
        b.Property(al => al.Action)
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(al => al.EntityType)
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(al => al.EntityId)
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(al => al.Details)
            .HasColumnType("jsonb")
            .IsRequired(false);
        
        b.Property(al => al.IpAddress)
            .HasColumnType("varchar")
            .IsRequired(false);
        
        b.Property(al => al.UserAgent)
            .HasColumnType("varchar")
            .IsRequired(false);
        
        b.Property(al => al.OccurredAt)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.HasIndex(al => al.UserId);
        b.HasIndex(al => al.OccurredAt);
        b.HasIndex(al => new { al.UserId, al.OccurredAt });

        b.ToTable(t =>
        {
            t.HasCheckConstraint("chk_auditlog_id_not_empty",
                "id != '00000000-0000-0000-0000-000000000000'");

            t.HasCheckConstraint("chk_auditlog_user_id_not_empty",
                "user_id != '00000000-0000-0000-0000-000000000000'");

            t.HasCheckConstraint("chk_auditlog_entity_type_not_empty",
                "entity_type IS NOT NULL AND length(trim(entity_type)) > 0");

            t.HasCheckConstraint("chk_auditlog_entity_id_not_empty",
                "entity_id IS NOT NULL AND length(trim(entity_id)) > 0");
        });
    }
}