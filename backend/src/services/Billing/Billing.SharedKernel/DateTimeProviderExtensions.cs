namespace Billing.SharedKernel;

public static class DateTimeProviderExtensions
{
    private const int DatabaseComparisonToleranceSeconds = 1;

    public static DateTimeOffset UtcNowForDatabaseComparison(this IDateTimeProvider provider)
        => provider.UtcNow.AddSeconds(DatabaseComparisonToleranceSeconds);

    public static DateTimeOffset FromDateTime(this IDateTimeProvider provider, DateTime dateTime)
        => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
}
