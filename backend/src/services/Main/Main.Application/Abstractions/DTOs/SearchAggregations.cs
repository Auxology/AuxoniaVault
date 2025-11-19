namespace Main.Application.Abstractions.DTOs;

public sealed record SearchAggregations
(
    Dictionary<string, long> ContentTypeCounts,
    Dictionary<string, long> DateHistogram
);