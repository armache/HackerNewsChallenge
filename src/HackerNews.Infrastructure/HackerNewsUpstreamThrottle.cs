using Microsoft.Extensions.Options;

namespace HackerNews.Infrastructure;

public sealed class HackerNewsUpstreamThrottle(IOptions<HackerNewsOptions> options)
{
    private readonly SemaphoreSlim throttler
        = new SemaphoreSlim(options.Value.MaxConcurrentUpstreamRequests, options.Value.MaxConcurrentUpstreamRequests);

    public Task WaitAsync(CancellationToken ct) => throttler.WaitAsync(ct);
    public void Release() => throttler.Release();
}
