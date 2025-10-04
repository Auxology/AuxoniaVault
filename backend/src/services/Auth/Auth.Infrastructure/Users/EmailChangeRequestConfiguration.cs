using Auth.Domain.Aggregates.User;
using Auth.Domain.Constants;
using Auth.Domain.Entities;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Users;

internal sealed class EmailChangeRequestConfiguration : IEntityTypeConfiguration<EmailChangeRequest>
{
    public void Configure(EntityTypeBuilder<EmailChangeRequest> b)
    {
        b.HasKey(ecr => ecr.Id);

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

        b.Property(ecr => ecr.CurrentEmail)
            .HasMaxLength(UserConstants.MaxEmailLength)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.UnsafeFromString(value)
            )
            .HasColumnType("citext")
            .IsRequired();

        b.Property(ecr => ecr.NewEmail)
            .HasMaxLength(UserConstants.MaxEmailLength)
            .HasConversion
            (
                email => email.Value,
                value => EmailAddress.UnsafeFromString(value)
            )
            .HasColumnType("citext")
            .IsRequired();

        b.Property(ecr => ecr.CurrentEmailOtp)
            .HasColumnType("text")
            .IsRequired(false);

        b.Property(ecr => ecr.NewEmailOtp)
            .HasColumnType("text")
            .IsRequired(false);

        b.Property(ecr => ecr.RequestedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        b.Property(ecr => ecr.ExpiresAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        b.Property(ecr => ecr.Method)
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();

        b.Property(ecr => ecr.CurrentStep)
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();

        b.HasOne<User>()
            .WithMany(u => u.EmailChangeRequests)
            .HasForeignKey(ecr => ecr.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}