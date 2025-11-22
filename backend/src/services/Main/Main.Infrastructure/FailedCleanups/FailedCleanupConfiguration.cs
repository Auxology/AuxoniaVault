using Main.Domain.Aggregates.FailedCleanup;
using Main.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Infrastructure.FailedCleanups;

internal sealed class FailedCleanupConfiguration : IEntityTypeConfiguration<FailedCleanup>
{
    public void Configure(EntityTypeBuilder<FailedCleanup> b)
    {
        b.HasKey(fc => fc.Id);

        b.Property(fc => fc.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("bigint")
            .IsRequired();

        b.Property(fm => fm.FileId)
            .HasConversion
            (
                id => id.Value,
                value => FileMetadataId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();
        
        b.Property(fm => fm.OwnerId)
            .HasConversion
            (
                id => id.Value,
                value => UserId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();
        
        b.Property(fm => fm.FileKey)
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(fm => fm.FailedAt)
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}