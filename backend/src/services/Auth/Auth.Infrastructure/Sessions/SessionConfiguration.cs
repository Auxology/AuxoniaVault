using Auth.Domain.Aggregates.Session;
using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Sessions;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> b)
    {
        b.HasKey(s => s.Id);

        b.Property(s => s.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => SessionId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();
        
        b.Property(s => s.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();
        
        b.Property(s => s.Token)
            .HasColumnType("text")
            .IsRequired();
        
        b.Property(s => s.IpAddress)
            .HasMaxLength(SessionConstants.MaxIpLength)
            .HasColumnType("text")
            .IsRequired();
        
        b.Property(s => s.UserAgent)
            .HasMaxLength(SessionConstants.MaxUserAgentLength)
            .HasColumnType("text")
            .IsRequired();
        
        b.Property(s => s.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(s => s.ExpiresAt)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(s => s.RevokedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);
        
        b.Property(s => s.Status)
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();
    }
}