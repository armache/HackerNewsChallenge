using FluentAssertions;
using HackerNews.Domain.Interfaces;
using HackerNews.Domain.Models;
using HackerNews.Infrastructure;
using HackerNews.Infrastructure.Cache;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace HackerNews.Api.UnitTests;

public class CachedHackerNewsClientTests
{
    private readonly Mock<IHackerNewsClient> hackerNewsClient = new();
    private readonly HybridCache cache = CreateHybridCache();

    private static HybridCache CreateHybridCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();

        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    private static HackerNewsOptions DefaultOptions() => new()
    {
        BaseUrl = "https://hacker-news.firebaseio.com/v0/",
        BestStoryIdsCacheDuration = TimeSpan.FromSeconds(60),
        StoryItemCacheDuration = TimeSpan.FromMinutes(5),
        MaxConcurrentUpstreamRequests = 20,
    };

    private CachedHackerNewsClient CreateSut(HackerNewsOptions? options = null) => new(
        hackerNewsClient.Object,
        cache,
        Options.Create(options ?? DefaultOptions()));

    [Fact]
    public async Task GetBestStoryIdsAsync_UseHackerNewsClientOnceWhileCached()
    {
        hackerNewsClient.Setup(i => i.GetBestStoryIdsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([1, 2, 3]);
        var sut = CreateSut();

        var first = await sut.GetBestStoryIdsAsync(CancellationToken.None);
        var second = await sut.GetBestStoryIdsAsync(CancellationToken.None);

        first.Should().Equal(second);
        hackerNewsClient.Verify(i => i.GetBestStoryIdsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStoryItemAsync_UseHackerNewsClientOnceWhileCached()
    {
        var item = new StoryItem { Id = 1, Score = 10 };
        hackerNewsClient.Setup(i => i.GetStoryItemAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        var sut = CreateSut();

        await sut.GetStoryItemAsync(1, CancellationToken.None);
        await sut.GetStoryItemAsync(1, CancellationToken.None);

        hackerNewsClient.Verify(i => i.GetStoryItemAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStoryItemAsync_DifferentIdsAreCachedIndependently()
    {
        hackerNewsClient.Setup(i => i.GetStoryItemAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new StoryItem { Id = 1, Score = 1 });
        hackerNewsClient.Setup(i => i.GetStoryItemAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new StoryItem { Id = 2, Score = 2 });

        var sut = CreateSut();

        var item1 = await sut.GetStoryItemAsync(1, CancellationToken.None);
        var item2 = await sut.GetStoryItemAsync(2, CancellationToken.None);

        item1!.Score.Should().Be(1);
        item2!.Score.Should().Be(2);
    }
}
