using Auth.Domain.Aggregates.LoginVerification;
using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.LoginVerifications;

internal sealed class LoginVerificationConfiguration : IEntityTypeConfiguration<LoginVerification>
{
    public void Configure(EntityTypeBuilder<LoginVerification> b)
    {
        b.HasKey(lv => lv.Id);
        
        b.Property(lv => lv.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => LoginVerificationId.UnsafeFromGuid(value))
            .HasColumnType("uuid")
            .IsRequired();
        
        b.Property(lv => lv.Identifier)
            .HasMaxLength(UserConstants.MaxEmailLength)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.UnsafeFromString(value))
            .HasColumnType("citext")
            .IsRequired();
        
        b.Property(lv => lv.Value)
            .HasColumnType("text")
            .IsRequired();
        
        b.Property(lv => lv.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(lv => lv.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        b.Property(lv => lv.ExpiresAt)
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}