using FluentAssertions;
using HackerNews.Api.Extensions;
using HackerNews.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace HackerNews.Api.UnitTests;

public class RateLimiterFactoryTests
{
    private static HttpContext NewHttpContext() => new DefaultHttpContext();

    private static RateLimitingOptions Options(int maxConcurrentRequests, int concurrencyQueueLimit) => new()
    {
        PermitLimit = 1000,
        WindowSeconds = 60,
        MaxConcurrentRequests = maxConcurrentRequests,
        ConcurrencyQueueLimit = concurrencyQueueLimit,
    };

    [Fact]
    public void AllowsRequestsUpToTheConfiguredConcurrencyLimit()
    {
        using var limiter = RateLimitingExtension.CreateGlobalLimiter(Options(maxConcurrentRequests: 2, concurrencyQueueLimit: 0));

        using var first = limiter.AttemptAcquire(NewHttpContext());
        using var second = limiter.AttemptAcquire(NewHttpContext());

        first.IsAcquired.Should().BeTrue();
        second.IsAcquired.Should().BeTrue();
    }

    [Fact]
    public async Task RejectsRequestsBeyondConcurrencyLimitAndQueueCapacity()
    {
        using var limiter = RateLimitingExtension.CreateGlobalLimiter(Options(maxConcurrentRequests: 2, concurrencyQueueLimit: 1));

        var first = limiter.AttemptAcquire(NewHttpContext());
        using var second = limiter.AttemptAcquire(NewHttpContext());

        var queuedLeaseTask = limiter.AcquireAsync(NewHttpContext(), cancellationToken: TestContext.Current.CancellationToken).AsTask();

        using var rejected = limiter.AttemptAcquire(NewHttpContext());

        first.IsAcquired.Should().BeTrue();
        second.IsAcquired.Should().BeTrue();
        rejected.IsAcquired.Should().BeFalse();

        first.Dispose();
        var queuedLease = await queuedLeaseTask;
        queuedLease.IsAcquired.Should().BeTrue();
        queuedLease.Dispose();
    }
}
