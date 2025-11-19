using Main.Infrastructure.Search.Models;
using OpenSearch.Client;

namespace Main.Infrastructure.Search.Services;

internal static class IndexMappings
{
    public static CreateIndexDescriptor GetFileMetadataIndexDescriptor(string indexName)
    {
        return new CreateIndexDescriptor(indexName)
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("filename_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "asciifolding", "filename_edge_ngram")
                        )
                    )
                    .TokenFilters(tf => tf
                        .EdgeNGram("filename_edge_ngram", en => en
                            .MinGram(2)
                            .MaxGram(20)
                        )
                    )
                )
            )
            .Map<FileMetadataDocument>(m => m
                .AutoMap()
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Id))
                    .Keyword(k => k.Name(n => n.OwnerId))
                    .Text(t => t
                        .Name(n => n.FileName)
                        .Analyzer("filename_analyzer")
                        .Fields(f => f
                            .Keyword(k => k.Name("keyword"))
                        )
                    )
                    .Keyword(k => k.Name(n => n.ContentType))
                    .Number(n => n.Name(nm => nm.FileSizeInBytes).Type(NumberType.Long))
                    .Keyword(k => k.Name(n => n.FileKey))
                    .Date(d => d.Name(n => n.CreatedAt))
                    .Date(d => d.Name(n => n.ModifiedAt))
                    .Text(t => t
                        .Name(n => n.FileDescription)
                        .Analyzer("standard")
                    )
                    .Boolean(b => b.Name(n => n.IsStarred))
                    .Completion(c => c
                        .Name(n => n.FileNameSuggest)
                    )
                )
            );
    }
}