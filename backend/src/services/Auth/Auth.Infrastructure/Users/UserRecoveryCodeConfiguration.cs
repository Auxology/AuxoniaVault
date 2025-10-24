using Auth.Domain.Aggregates.User;
using Auth.Domain.Entities;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Users;

internal sealed class UserRecoveryCodeConfiguration : IEntityTypeConfiguration<UserRecoveryCode>
{
    public void Configure(EntityTypeBuilder<UserRecoveryCode> b)
    {
        b.HasKey(urc => urc.Id);
        
        b.Property(ecr => ecr.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("bigint")
            .IsRequired();

        b.Property(ecr => ecr.UserId)
            .HasConversion
            (
                id => id.Value,
                value => UserId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();
        
        b.Property(ecr => ecr.HashedCode)
            .HasColumnType("text")
            .IsRequired();
        
        b.Property(ecr => ecr.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(ecr => ecr.LastUsedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        b.HasIndex(urc => new { urc.UserId, urc.HashedCode });
        
        b.HasOne<User>()
            .WithMany(u => u.RecoveryCodes)
            .HasForeignKey(ecr => ecr.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}