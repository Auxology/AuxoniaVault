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
            .HasColumnType("bigint");

        b.Property(we => we.StripeEventId)
            .IsRequired()
            .HasColumnType("text");
        
        b.Property(we => we.StripeEventType)
            .IsRequired()
            .HasColumnType("varchar");

        b.Property(we => we.Status)
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();
        
        b.Property(we => we.RetryCount)
            .IsRequired()
            .HasColumnType("int");
        
        b.Property(we => we.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");
        
        b.Property(we => we.FailedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);
        
        b.Property(we => we.ProcessedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);
    }
}