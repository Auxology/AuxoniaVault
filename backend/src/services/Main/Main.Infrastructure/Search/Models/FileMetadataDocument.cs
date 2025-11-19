using System.ComponentModel.DataAnnotations;
using OpenSearch.Client;

namespace Main.Infrastructure.Search.Models;

internal sealed class FileMetadataDocument
{
    [Keyword] public string Id { get; set; } = string.Empty;
    
    [Keyword] public string OwnerId { get; set; } = string.Empty;

    [Text(Analyzer = "filename_analyzer")]
    [Keyword]
    public string FileName { get; set; } = string.Empty;
    
    [Text(Analyzer = "standard")]
    public string? FileDescription { get; set; }
    
    [Keyword]
    public string ContentType { get; set; } = string.Empty;
    
    [Number(NumberType.Long)]
    public long FileSizeInBytes { get; set; }
    
    [Key]
    public string FileKey { get; set; } = string.Empty;
    
    [Date]
    public DateTimeOffset CreatedAt { get; set; }
    
    [Date]
    public DateTimeOffset? ModifiedAt { get; set; }
    
    [Boolean]
    public bool IsStarred { get; set; }
    
    [Completion]
    public CompletionField? FileNameSuggest { get; set; }
}