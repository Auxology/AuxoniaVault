using Auth.Domain.Aggregates.User;
using Auth.Domain.Constants;
using Auth.Domain.Entities;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Users;

internal sealed class UserRecoveryRequestConfiguration : IEntityTypeConfiguration<UserRecoveryRequest>
{
    public void Configure(EntityTypeBuilder<UserRecoveryRequest> b)
    {
        b.HasKey(urq => urq.Id);
        
        b.Property(urq => urq.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("bigint")
            .IsRequired();

        b.Property(urq => urq.UserId)
            .HasConversion
            (
                id => id.Value,
                value => UserId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();
        
        b.Property(urq => urq.UniqueIdentifier)
            .HasColumnType("text")
            .IsRequired();
        
        b.Property(urq => urq.ApprovedAt)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(urq => urq.ExpiresAt)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(urq => urq.IsCompleted)
            .HasColumnType("boolean")
            .IsRequired();
        
        b.Property(urq => urq.NewEmail)
            .HasMaxLength(UserConstants.MaxEmailLength)
            .HasColumnType("citext")
            .IsRequired(false);
        
        b.Property(urq => urq.CompletedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        b.HasIndex(urq => urq.UniqueIdentifier)
            .IsUnique();

        b.HasIndex(urq => new { urq.UserId, urq.ExpiresAt })
            .HasFilter("\"is_completed\" = FALSE")
            .IsDescending(false, true)
            .IncludeProperties(urq => new { urq.UniqueIdentifier, urq.NewEmail });

        b.HasIndex(urq => new { urq.UserId, urq.CompletedAt })
            .HasFilter("\"is_completed\" = TRUE")
            .IsDescending(false, true);
        
        b.HasOne<User>()
            .WithMany(u => u.RecoveryRequests)
            .HasForeignKey(ecr => ecr.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();    
    }
}