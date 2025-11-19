namespace Main.Application.Abstractions.DTOs;

public sealed record SearchResult<T>
(
    IReadOnlyList<T> Items,
    long TotalCount,
    SearchAggregations Aggregations
);