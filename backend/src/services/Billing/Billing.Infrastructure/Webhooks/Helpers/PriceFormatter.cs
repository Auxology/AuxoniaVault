using Billing.Infrastructure.Webhooks.Constants;

namespace Billing.Infrastructure.Webhooks.Helpers;

internal static class PriceFormatter
{
    
    public static string Format(long? unitAmountInCents, string? currency)
    {
        if (!unitAmountInCents.HasValue)
            return WebhookConstants.DefaultPrice;
        
        string currencyCode = (currency ?? "USD").ToUpperInvariant();
        
        if (IsZeroDecimalCurrency(currencyCode))
            return FormatZeroDecimalCurrency(unitAmountInCents.Value, currencyCode);
        
        decimal amount = unitAmountInCents.Value / 100.0m;
        
        return currencyCode switch
        {
            "USD" => $"${amount:F2}",
            "EUR" => $"€{amount:F2}",
            "GBP" => $"£{amount:F2}",
            "CAD" => $"CA${amount:F2}",
            "AUD" => $"AU${amount:F2}",
            _ => $"{amount:F2} {currencyCode}"
        };    
    }
    
    private static bool IsZeroDecimalCurrency(string currency)
    {
        string[] zeroDecimalCurrencies = { "JPY", "KRW", "VND", "CLP", "TWD" };
        return Array.Exists(zeroDecimalCurrencies, c => c == currency);
    }

    private static string FormatZeroDecimalCurrency(long amount, string currency)
    {
        return currency switch
        {
            "JPY" => $"¥{amount:N0}",
            "KRW" => $"₩{amount:N0}",
            _ => $"{amount:N0} {currency}"
        };
    }
}