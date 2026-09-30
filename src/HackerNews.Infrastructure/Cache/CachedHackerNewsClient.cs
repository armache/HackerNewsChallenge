using HackerNews.Domain.Interfaces;
using HackerNews.Domain.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace HackerNews.Infrastructure.Cache;

public sealed class CachedHackerNewsClient(
    IHackerNewsClient hackerNewsClient,
    HybridCache cache,
    IOptions<HackerNewsOptions> options) : IHackerNewsClient
{
    private readonly HybridCacheEntryOptions bestStoryIdsCacheOptions = new HybridCacheEntryOptions
    {
        Expiration = options.Value.BestStoryIdsCacheDuration,
        LocalCacheExpiration = options.Value.BestStoryIdsCacheDuration,
    };

    private readonly HybridCacheEntryOptions itemCacheOptions = new HybridCacheEntryOptions
    {
        Expiration = options.Value.StoryItemCacheDuration,
        LocalCacheExpiration = options.Value.StoryItemCacheDuration
    };

    public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken ct)
    {
        return await cache.GetOrCreateAsync(
            "beststories",
            hackerNewsClient,
            (state, cancellationToken) => new ValueTask<IReadOnlyList<int>>(state.GetBestStoryIdsAsync(cancellationToken)),
            bestStoryIdsCacheOptions,
            cancellationToken: ct);
    }

    public async Task<StoryItem?> GetStoryItemAsync(int id, CancellationToken ct)
    {
        return await cache.GetOrCreateAsync(
            $"item:{id}",
            (hackerNewsClient, id),
            (state, cancellationToken) => new ValueTask<StoryItem?>(state.hackerNewsClient.GetStoryItemAsync(state.id, cancellationToken)),
            itemCacheOptions,
            cancellationToken: ct);
    }
}
