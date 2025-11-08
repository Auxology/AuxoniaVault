namespace Main.Domain.Objects;

public sealed record PartETag
(
    int PartNumber,
    string ETag
);