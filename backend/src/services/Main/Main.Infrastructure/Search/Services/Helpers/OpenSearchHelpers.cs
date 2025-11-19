using Main.Application.Abstractions.DTOs;
using Main.Domain.Aggregates.FileMetadata;
using Main.Infrastructure.Search.Models;
using OpenSearch.Client;

namespace Main.Infrastructure.Search.Services.Helpers;

internal static class OpenSearchHelpers
{
    public static FileMetadataDocument MapToDocument(FileMetadata fileMetadata)
    {
        return new FileMetadataDocument
        {
            Id = fileMetadata.Id.Value.ToString(),
            OwnerId = fileMetadata.OwnerId.Value.ToString(),
            FileName = fileMetadata.FileName,
            ContentType = fileMetadata.ContentType,
            FileSizeInBytes = fileMetadata.FileSizeInBytes,
            FileKey = fileMetadata.FileKey,
            CreatedAt = fileMetadata.CreatedAt,
            ModifiedAt = fileMetadata.ModifiedAt,
            FileDescription = fileMetadata.Description,
            IsStarred = fileMetadata.IsStarred,
            FileNameSuggest = new CompletionField
            {
                Input = new[] { fileMetadata.FileName }
            }
        };
    }

    public static FileMetadataSearchResult MapToSearchResult(FileMetadataDocument doc, double score)
    {
        return new FileMetadataSearchResult
        (
            Id: Guid.Parse(doc.Id),
            OwnerId: Guid.Parse(doc.OwnerId),
            FileName: doc.FileName,
            ContentType: doc.ContentType,
            SizeInBytes: doc.FileSizeInBytes,
            FileKey: doc.FileKey,
            CreatedAt: doc.CreatedAt,
            ModifiedAt: doc.ModifiedAt,
            Description: doc.FileDescription,
            doc.IsStarred,
            score
        );
    }

    public static QueryContainer BuildFilesQuery(QueryContainerDescriptor<FileMetadataDocument> q, Guid ownerId,
        string? searchTerm, SearchFilters filters)
    {
        var mustQueries = new List<QueryContainer>
        {
            q.Term(t => t.OwnerId, ownerId.ToString())
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            mustQueries.Add(q.MultiMatch(mm => mm
                .Fields(f => f
                    .Field(fd => fd.FileName)
                    .Field(fd => fd.FileDescription))
                .Query(searchTerm)
                .Fuzziness(Fuzziness.Auto)
                .Type(TextQueryType.BestFields)));
        }

        if (filters.ContentTypes?.Any() == true)
        {
            mustQueries.Add(q.Terms(t => t.Field(f => f.ContentType).Terms(filters.ContentTypes)));
        }
        
        if (filters.IsStarred.HasValue)
        {
            mustQueries.Add(q.Term(t => t.Field(f => f.IsStarred).Value(filters.IsStarred.Value)));
        }
        
        if (filters.CreatedBefore.HasValue)
        {
            mustQueries.Add(q.DateRange(dr => dr
                .Field(f => f.CreatedAt)
                .LessThanOrEquals(filters.CreatedBefore.Value.UtcDateTime)));
        }
        
        if (filters.CreatedAfter.HasValue)
        {
            mustQueries.Add(q.DateRange(dr => dr
                .Field(f => f.CreatedAt)
                .GreaterThanOrEquals(filters.CreatedAfter.Value.UtcDateTime)));
        }
        
        if (filters.MinFileSizeInBytes.HasValue || filters.MaxFileSizeInBytes.HasValue)
        {
            mustQueries.Add(q.Range(r => r
                .Field(f => f.FileSizeInBytes)
                .GreaterThanOrEquals(filters.MinFileSizeInBytes)
                .LessThanOrEquals(filters.MaxFileSizeInBytes)));
        }
        
        return q.Bool(b => b.Must(mustQueries.ToArray()));
    }

    public static SortDescriptor<FileMetadataDocument> BuildFileSort(
        SortDescriptor<FileMetadataDocument> s, string sortBy)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "name" => s.Ascending(f => f.FileName.Suffix("keyword")),
            "name_desc" => s.Descending(f => f.FileName.Suffix("keyword")),
            "date" => s.Ascending(f => f.CreatedAt),
            "date_desc" => s.Descending(f => f.CreatedAt),
            "size" => s.Ascending(f => f.FileSizeInBytes),
            "size_desc" => s.Descending(f => f.FileSizeInBytes),
            _ => s.Descending(SortSpecialField.Score)
        };
    }

    public static AggregationContainerDescriptor<FileMetadataDocument> BuildFileAggregation(
        AggregationContainerDescriptor<FileMetadataDocument> a)
    {
        return a
            .Terms("content_types", t => t.Field(f => f.ContentType).Size(50))
            .DateHistogram("creation_dates", dh => dh
                .Field(f => f.CreatedAt)
                .CalendarInterval(DateInterval.Day)
            );
    }
    
    public static SearchAggregations ExtractAggregations(AggregateDictionary aggregations)
    {
        var contentTypeCounts = new Dictionary<string, long>();
        var dateHistogram = new Dictionary<string, long>();

        if (aggregations.Terms("content_types") is { } contentTypeAgg)
        {
            foreach (var bucket in contentTypeAgg.Buckets)
            {
                contentTypeCounts[bucket.Key] = bucket.DocCount ?? 0;
            }
        }

        if (aggregations.DateHistogram("creation_dates") is { } dateAgg)
        {
            foreach (var bucket in dateAgg.Buckets)
            {
                dateHistogram[bucket.Date.ToString("yyyy-MM-dd")] = bucket.DocCount ?? 0;
            }
        }

        return new SearchAggregations(contentTypeCounts, dateHistogram);
    }
}