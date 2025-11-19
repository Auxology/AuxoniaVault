namespace Main.Infrastructure.Search.DTOs;

public sealed record SearchResult<T>
(
    IReadOnlyList<T> Items,
    long TotalCount,
    SearchAggregations Aggregations
);