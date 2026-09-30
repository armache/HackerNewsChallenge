using HackerNews.Domain.Interfaces;
using HackerNews.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HackerNews.Domain.Services;

public class StoryService(IHackerNewsClient client, ILogger<StoryService> logger) : IStoryService
{
    public async Task<IReadOnlyList<StoryDto>> GetBestStoriesAsync(int count, CancellationToken ct)
    {
        if (count <= 0) return [];

        var ids = await client.GetBestStoryIdsAsync(ct);

        if (ids.Count == 0) return [];

        var items = await GetItemsAsync(ids, ct);

        return items
            .Where(item => item is { Deleted: false, Dead: false })
            .OrderByDescending(item => item.Score)
            .Take(count)
            .Select(ToDto)
            .ToList();
    }

    private async Task<List<StoryItem>> GetItemsAsync(IReadOnlyList<int> ids, CancellationToken ct)
    {
        var fetchTasks = ids.Select(async id =>
        {
            try
            {
                return await client.GetStoryItemAsync(id, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Failed to fetch Hacker News item {ItemId}", id);
                return null;
            }
        });

        var items = await Task.WhenAll(fetchTasks);

        return items.OfType<StoryItem>().ToList();
    }

    private static StoryDto ToDto(StoryItem item) => new()
    {
        Title = item.Title ?? string.Empty,
        Uri = item.Url ?? string.Empty,
        PostedBy = item.By ?? string.Empty,
        Time = DateTimeOffset.FromUnixTimeSeconds(item.Time),
        Score = item.Score,
        CommentCount = item.CommentCount,
    };
}
