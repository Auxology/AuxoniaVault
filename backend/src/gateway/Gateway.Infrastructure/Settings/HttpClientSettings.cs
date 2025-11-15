namespace Gateway.Infrastructure.Settings;

public sealed class HttpClientSettings
{
    public const string SectionName = nameof(HttpClientSettings);
    
    public string AuthServiceBaseUrl { get; init; }
    
    public string AuthServiceVerifyLoginEndpoint { get; init; }
    
    public string AuthServiceLoginWithRefreshTokenEndpoint { get; init; }
}