namespace Main.Infrastructure.Search.DTOs;

public sealed record SearchAggregations
(
    Dictionary<string, long> ContentTypeCounts,
    Dictionary<string, long> DateHistogram
);