using Main.SharedKernel;

namespace Main.Domain.ValueObjects;

public readonly record struct FileMetadataId
{
    private static Error Invalid => Error.Validation(
        "FileMetadataId.Invalid",
        "FileMetadataId cannot be an empty GUID."
    );

    private static Error StringRequired => Error.Validation(
        "FileMetadataId.StringRequired",
        "FileMetadataId requires a non-empty string that can be parsed as a valid GUID."
    );

    public Guid Value { get; }

    private FileMetadataId(Guid value)
    {
        Value = value;
    }

    public static FileMetadataId New() => new(Guid.NewGuid());

    public static Result<FileMetadataId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<FileMetadataId>(Invalid);

        return Result.Success(new FileMetadataId(value));
    }

    public static FileMetadataId UnsafeFromGuid(Guid value) => new(value);

    public static Result<FileMetadataId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FileMetadataId>(StringRequired);

        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
            return Result.Failure<FileMetadataId>(Invalid);

        return Result.Success(FromGuid(guid).Value);
    }

    public override string ToString() => Value.ToString();

    public bool IsEmpty() => Value == Guid.Empty;

    public static implicit operator Guid(FileMetadataId fileMetadataId) => fileMetadataId.Value;

    public static implicit operator FileMetadataId(Guid value) => FromGuid(value).Value;
}