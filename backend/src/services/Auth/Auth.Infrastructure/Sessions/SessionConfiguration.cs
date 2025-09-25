using Auth.Domain.Aggregates.Session;
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
            .HasMaxLength(45)
            .HasColumnType("text")
            .IsRequired();
        
        b.Property(s => s.UserAgent)
            .HasMaxLength(512)
            .HasColumnType("text")
            .IsRequired();
        
        b.Property(s => s.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(s => s.ExpiresAt)
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}