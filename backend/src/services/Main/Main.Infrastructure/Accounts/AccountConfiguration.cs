using Main.Domain.Aggregates.Account;
using Main.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Infrastructure.Accounts;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.HasKey(a => a.Id);
        
        b.Property(a => a.Id)
            .HasConversion
            (
                id => id.Value,
                value => UserId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();

        b.Property(a => a.AccountName)
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(a => a.AccountEmail)
            .HasColumnType("citext")
            .IsRequired();
        
        b.Property(a => a.AccountTier)
            .HasColumnType("int")
            .IsRequired();
        
        b.Property(a => a.MaxStorageInBytes)
            .HasColumnType("bigint")
            .IsRequired();
        
        b.Property(a => a.UsedStorageInBytes)
            .HasColumnType("bigint")
            .IsRequired();
        
        b.Property(a => a.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(a => a.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);
    }
}