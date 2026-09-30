using HackerNews.Domain.Interfaces;
using HackerNews.Domain.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace HackerNews.Infrastructure.HttpClients;

public sealed class HackerNewsClient(HttpClient httpClient, HackerNewsUpstreamThrottle requestThrottle) : IHackerNewsClient
{
    private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken ct)
    {
        return await httpClient.GetFromJsonAsync<int[]>("beststories.json", serializerOptions, ct) ?? [];
    }

    public async Task<StoryItem?> GetStoryItemAsync(int id, CancellationToken ct)
    {
        await requestThrottle.WaitAsync(ct);
        try
        {
            return await httpClient.GetFromJsonAsync<StoryItem>($"item/{id}.json", serializerOptions, ct);
        }
        finally
        {
            requestThrottle.Release();
        }
    }
}
