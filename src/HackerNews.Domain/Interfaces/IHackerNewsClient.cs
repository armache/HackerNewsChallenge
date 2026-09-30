using HackerNews.Domain.Models;

namespace HackerNews.Domain.Interfaces;

public interface IHackerNewsClient
{
    Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken ct);
    Task<StoryItem?> GetStoryItemAsync(int id, CancellationToken ct);
}
