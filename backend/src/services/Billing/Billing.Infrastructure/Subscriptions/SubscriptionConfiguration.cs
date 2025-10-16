using Billing.Domain.Aggregate.Customer;
using Billing.Domain.Entities;
using Billing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Subscriptions;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> b)
    {
        b.HasKey(s => s.Id);

        b.Property(s => s.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("bigint")
            .IsRequired();
        
        b.Property(s => s.StripeCustomerId)
            .HasColumnType("text")
            .IsRequired();

        b.Property(s => s.UserId)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => UserId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();

        b.Property(s => s.StripePriceId)
            .HasColumnType("text")
            .IsRequired();

        b.Property(s => s.StripeSubscriptionId)
            .HasColumnType("text")
            .IsRequired();
        
        b.Property(s => s.Status)
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();

        b.Property(s => s.CurrentPeriodStart)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(s => s.CurrentPeriodEnd)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(s => s.CancelAtPeriodEnd)
            .HasColumnType("boolean")
            .IsRequired();
        
        b.Property(s => s.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        b.Property(s => s.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        b.HasOne<Customer>()
            .WithMany(c => c.Subscriptions)
            .HasForeignKey(s => s.StripeCustomerId)
            .HasPrincipalKey(c => c.StripeCustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}