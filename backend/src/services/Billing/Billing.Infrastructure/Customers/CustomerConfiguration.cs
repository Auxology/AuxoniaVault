using Billing.Domain.Aggregate.Customer;
using Billing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Customers;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.HasKey(c => c.Id);

        b.Property(c => c.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("bigint")
            .IsRequired();
        
        b.Property(u => u.UserId)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => UserId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();
        
        b.Property(c => c.StripeCustomerId)
            .HasColumnType("text")
            .IsRequired();
        
        b.Property(c => c.StripeCustomerName)
            .HasColumnType("varchar")
            .IsRequired(); 
        
        b.Property(c => c.StripeCustomerEmail)
            .HasColumnType("text")
            .IsRequired();

        b.HasIndex(c => c.UserId)
            .IsUnique();
        b.HasIndex(c => c.StripeCustomerId)
            .IsUnique();

        b.HasIndex(c => c.StripeCustomerEmail);

        b.Navigation(c => c.Subscriptions).AutoInclude(false);
    }
}