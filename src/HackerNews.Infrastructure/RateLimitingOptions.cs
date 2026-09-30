namespace HackerNews.Infrastructure;

public sealed class RateLimitingOptions
{
    public int PermitLimit { get; set; } = 100;

    public int WindowSeconds { get; set; } = 60;

    public int QueueLimit { get; set; } = 0;

    public int MaxConcurrentRequests { get; set; } = 50;

    public int ConcurrencyQueueLimit { get; set; } = 20;
}
