using Billing.Domain.Errors;
using Billing.SharedKernel;

namespace Billing.Domain.Entities;

public class SubscriptionHistory : Entity
{
    public int Id { get; private set; }
    
    public string StripeSubscriptionId { get; private set; }
    
    public string ProductName { get; private set; }
    
    public string PriceFormatted { get; private set; }
    
    public string EventType { get; private set; }
    
    public DateTimeOffset PeriodStart { get; private set; }
    
    public DateTimeOffset PeriodEnd { get; private set; }
    
    
    private SubscriptionHistory() {} // For EF core

    private SubscriptionHistory
    (
        string stripeSubscriptionId,
        string productName,
        string priceFormatted,
        string eventType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd
    )

    {
        StripeSubscriptionId = stripeSubscriptionId;
        ProductName = productName;
        PriceFormatted = priceFormatted;
        EventType = eventType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
    }
    
    public static Result<SubscriptionHistory> Create
    (
        string stripeSubscriptionId,
        string productName,
        string priceFormatted,
        string eventType,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd
    )
    {
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
            return Result.Failure<SubscriptionHistory>(SubscriptionHistoryErrors.StripeSubscriptionIdRequired);
        
        if (string.IsNullOrWhiteSpace(productName))
            return Result.Failure<SubscriptionHistory>(SubscriptionHistoryErrors.ProductNameRequired);
        
        if (string.IsNullOrWhiteSpace(priceFormatted))
            return Result.Failure<SubscriptionHistory>(SubscriptionHistoryErrors.PriceFormattedRequired);
        
        var subscriptionHistory = new SubscriptionHistory
        (
            stripeSubscriptionId,
            productName,
            priceFormatted,
            eventType,
            periodStart,
            periodEnd
        );
        
        return Result.Success(subscriptionHistory);
    }
}