using HackerNews.Domain.Models;

namespace HackerNews.Domain.Interfaces;

public interface IStoryService
{
    Task<IReadOnlyList<StoryDto>> GetBestStoriesAsync(int count, CancellationToken ct);
}
