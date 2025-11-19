using Main.Domain.Aggregates.FileMetadata;
using Main.Domain.Constants;
using Main.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Infrastructure.Files;

internal sealed class FileMetadataConfiguration : IEntityTypeConfiguration<FileMetadata>
{
    public void Configure(EntityTypeBuilder<FileMetadata> b)
    {
        b.HasKey(fm => fm.Id);
        
        b.Property(fm => fm.Id)
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
        
        b.Property(fm => fm.FileName)
            .HasColumnType("varchar")
            .HasMaxLength(FileConstants.MaxFileNameLength)
            .IsRequired();
        
        b.Property(fm => fm.ContentType)
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(fm => fm.FileSizeInBytes)
            .HasColumnType("bigint")
            .IsRequired();
        
        b.Property(fm => fm.FileKey)
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(fm => fm.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(fm => fm.ModifiedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);
        
        b.Property(fm => fm.Description)
            .HasColumnType("varchar")
            .HasMaxLength(FileConstants.MaxDescriptionLength)
            .IsRequired(false);
        
        b.Property(fm => fm.IsStarred)
            .HasColumnType("boolean")
            .IsRequired();
    }
}