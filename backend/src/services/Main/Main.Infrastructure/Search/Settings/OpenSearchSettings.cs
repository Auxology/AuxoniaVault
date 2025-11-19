namespace Main.Infrastructure.Search.Settings;

internal sealed class OpenSearchSettings
{
    public const string SectionName = "OpenSearch";
    
    public string Uri { get; init; } = string.Empty;
    
    public string DefaultIndex { get; init; } = string.Empty;
    
    public int ConnectionTimeout { get; init; } = 30;
    
    public int RequestTimeout { get; init; } = 30;
}