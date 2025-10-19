using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Customers;

internal sealed class SubscriptionHistoryConfiguration : IEntityTypeConfiguration<SubscriptionHistory>
{
    public void Configure(EntityTypeBuilder<SubscriptionHistory> b)
    {
        b.HasKey(sh => sh.Id);
        
        b.Property(sh => sh.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("bigint")
            .IsRequired();
        
        b.Property(sh => sh.StripeSubscriptionId)
            .HasColumnType("text")
            .IsRequired();
        
        
        b.Property(sh => sh.ProductName)
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(sh => sh.PriceFormatted)
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(sh => sh.EventType)
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(sh => sh.PeriodStart)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(sh => sh.PeriodEnd)
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}