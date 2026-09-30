namespace HackerNews.Infrastructure;

public sealed record HackerNewsOptions
{
    public required string BaseUrl { get; set; }
    public required TimeSpan BestStoryIdsCacheDuration { get; set; }
    public required TimeSpan StoryItemCacheDuration { get; set; }
    public required int MaxConcurrentUpstreamRequests { get; set; }
}
