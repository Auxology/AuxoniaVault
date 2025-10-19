using Billing.Domain.Aggregate.Webhook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.WebhookEvents;

internal sealed class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> b)
    {
        b.HasKey(we => we.Id);

        b.Property(we => we.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("bigint")
            .IsRequired();

        b.Property(we => we.StripeEventId)
            .HasColumnType("text") 
            .IsRequired();

        b.Property(we => we.StripeEventType)
            .HasColumnType("varchar")
            .IsRequired();

        b.Property(we => we.Status)
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(we => we.RetryCount)
            .HasColumnType("int")
            .IsRequired();

        b.Property(we => we.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();
        
        b.Property(we => we.FailedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);
        
        b.Property(we => we.ProcessedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);
    }
}